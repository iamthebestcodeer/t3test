using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Messages;

namespace T3Code.Transport.Client;

public sealed class WebSocketTransportClient : ITransportClient
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource<RpcResponse>> _pendingRequests = new();
    private readonly ConcurrentDictionary<string, List<Action<JsonElement>>> _subscriptions = new();
    private readonly ConcurrentDictionary<string, Action<JsonElement>> _domainEventHandlers = new();
    private readonly ConcurrentDictionary<string, object?> _subscriptionParameters = new();
    private readonly object _stateLock = new();
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _reconnectDelay;
    private readonly int _maxReconnectAttempts;

    private ClientWebSocket? _webSocket;
    private ConnectionInfo? _connectionInfo;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoop;
    private ConnectionStateSnapshot _state = ConnectionStateSnapshot.Disconnected();
    private long _nextRequestId;
    private volatile bool _disposed;

    public ConnectionStateSnapshot ConnectionState
    {
        get { lock (_stateLock) return _state; }
    }

    public event Action<ConnectionStateSnapshot>? ConnectionStateChanged;

    public WebSocketTransportClient(
        TimeSpan? reconnectDelay = null,
        int maxReconnectAttempts = 50)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
        _reconnectDelay = reconnectDelay ?? TimeSpan.FromMilliseconds(250);
        _maxReconnectAttempts = maxReconnectAttempts;
    }

    public async Task ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _connectionInfo = connectionInfo;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        UpdateState(ConnectionStateSnapshot.Connecting(0));
        await ConnectInternalAsync(_cts.Token);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        _cts?.Cancel();
        UpdateState(ConnectionStateSnapshot.Disconnected());

        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnect",
                    cancellationToken);
            }
            catch
            {
                // Swallow close errors during disconnect
            }
        }

        _webSocket?.Dispose();
        _webSocket = null;
    }

    public async Task<TResponse> RequestAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync(method, parameters, cancellationToken);

        if (response.Error != null)
        {
            throw new RpcException(response.Error.Code, response.Error.Message);
        }

        if (response.Result == null)
        {
            throw new RpcException(-1, $"Null result for method '{method}'");
        }

        return response.Result.Value.Deserialize<TResponse>(_jsonOptions)
            ?? throw new RpcException(-1, $"Failed to deserialize result for method '{method}'");
    }

    public async Task<IReadOnlyList<TResponse>> RequestListAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync(method, parameters, cancellationToken);

        if (response.Error != null)
        {
            throw new RpcException(response.Error.Code, response.Error.Message);
        }

        if (response.Result == null)
        {
            return [];
        }

        return response.Result.Value.Deserialize<List<TResponse>>(_jsonOptions)
            ?? [];
    }

    public async Task<ISubscription<TItem>> SubscribeAsync<TItem>(
        string method,
        Action<TItem> handler,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var subs = _subscriptions.GetOrAdd(method, _ => new List<Action<JsonElement>>());

        Action<JsonElement> jsonHandler = element =>
        {
            try
            {
                var item = element.Deserialize<TItem>(_jsonOptions);
                if (item != null)
                {
                    handler(item);
                }
            }
            catch
            {
                // Swallow deserialization errors in subscription handlers
            }
        };

        lock (subs)
        {
            subs.Add(jsonHandler);
        }

        // Store parameters for replay on reconnect (only store first registration per method)
        _subscriptionParameters.TryAdd(method, parameters);

        // Send the subscription request to the server
        await SendRequestAsync(method, parameters, cancellationToken);

        return new Subscription<TItem>(() =>
        {
            lock (subs)
            {
                subs.Remove(jsonHandler);
            }
        });
    }

    public void RegisterDomainEventHandler(string eventType, Action<JsonElement> handler)
    {
        _domainEventHandlers[eventType] = handler;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();

        _webSocket?.Dispose();
        _webSocket = null;

        // Complete all pending requests with cancellation
        foreach (var kvp in _pendingRequests)
        {
            kvp.Value.TrySetCanceled();
        }
        _pendingRequests.Clear();

        await Task.CompletedTask;
    }

    private async Task ConnectInternalAsync(CancellationToken cancellationToken)
    {
        if (_connectionInfo == null)
        {
            throw new InvalidOperationException("ConnectionInfo not set");
        }

        _webSocket?.Dispose();
        _webSocket = new ClientWebSocket();

        var uri = new Uri(_connectionInfo.WebSocketUrl);

        try
        {
            await _webSocket.ConnectAsync(uri, cancellationToken);
            UpdateState(ConnectionStateSnapshot.Connected());

            // Replay subscriptions after successful reconnect
            await ReplaySubscriptionsAsync(cancellationToken);

            _receiveLoop = Task.Run(() => ReceiveLoopAsync(cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            UpdateState(ConnectionStateSnapshot.Failed(ex.Message));
            StartReconnectLoop();
        }
    }

    private async Task ReplaySubscriptionsAsync(CancellationToken cancellationToken)
    {
        foreach (var kvp in _subscriptionParameters)
        {
            try
            {
                await SendRequestAsync(kvp.Key, kvp.Value, cancellationToken);
            }
            catch
            {
                // Swallow errors during replay - individual subscription failures should not break reconnect
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var messageBuffer = new StringBuilder();

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   _webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    UpdateState(ConnectionStateSnapshot.Reconnecting(0, "Server closed connection"));
                    StartReconnectLoop();
                    return;
                }

                messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var messageJson = messageBuffer.ToString();
                    messageBuffer.Clear();
                    ProcessMessage(messageJson);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (WebSocketException ex) when (ex.WebSocketErrorCode != WebSocketError.InvalidState)
        {
            UpdateState(ConnectionStateSnapshot.Reconnecting(0, ex.Message));
            StartReconnectLoop();
        }
        catch (Exception ex)
        {
            UpdateState(ConnectionStateSnapshot.Failed(ex.Message));
            StartReconnectLoop();
        }
    }

    internal void ProcessMessage(string json)
    {
        try
        {
            var message = JsonSerializer.Deserialize<RpcMessage>(json, _jsonOptions);
            if (message == null) return;

            if (message.IsResponse && message.Id.HasValue)
            {
                var response = new RpcResponse
                {
                    Id = message.Id.Value,
                    Result = message.Result,
                    Error = message.Error,
                };

                if (_pendingRequests.TryRemove(message.Id.Value, out var tcs))
                {
                    tcs.TrySetResult(response);
                }
            }
            else if (message.IsPush && message.Method != null)
            {
                HandlePush(message.Method, message.Params);
            }
        }
        catch
        {
            // Swallow malformed messages
        }
    }

    private void HandlePush(string method, JsonElement? parameters)
    {
        // Handle domain events
        if (parameters.HasValue)
        {
            if (_domainEventHandlers.TryGetValue(method, out var handler))
            {
                handler(parameters.Value);
            }
        }

        // Handle subscription pushes
        if (_subscriptions.TryGetValue(method, out var subs))
        {
            Action<JsonElement>[] handlers;
            lock (subs)
            {
                handlers = subs.ToArray();
            }

            foreach (var handler in handlers)
            {
                if (parameters.HasValue)
                {
                    handler(parameters.Value);
                }
            }
        }
    }

    private async Task<RpcResponse> SendRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (_disposed || _webSocket?.State != WebSocketState.Open)
        {
            throw new RpcException(-1, "Not connected");
        }

        var id = Interlocked.Increment(ref _nextRequestId);
        var request = new RpcRequest
        {
            Id = id,
            Method = method,
            Params = parameters,
        };

        var tcs = new TaskCompletionSource<RpcResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[id] = tcs;

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        try
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
        catch
        {
            _pendingRequests.TryRemove(id, out _);
            throw;
        }

        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    private void StartReconnectLoop()
    {
        Task.Run(async () =>
        {
            for (var attempt = 1; attempt <= _maxReconnectAttempts; attempt++)
            {
                if (_disposed || _cts?.IsCancellationRequested == true) return;

                UpdateState(ConnectionStateSnapshot.Reconnecting(attempt));

                try
                {
                    await Task.Delay(_reconnectDelay * attempt, _cts?.Token ?? CancellationToken.None);
                    await ConnectInternalAsync(_cts?.Token ?? CancellationToken.None);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // Continue retrying
                }
            }

            UpdateState(ConnectionStateSnapshot.Failed("Max reconnect attempts exceeded"));
        });
    }

    private void UpdateState(ConnectionStateSnapshot state)
    {
        lock (_stateLock)
        {
            _state = state;
        }
        ConnectionStateChanged?.Invoke(state);
    }
}

public sealed class RpcException : Exception
{
    public int Code { get; }

    public RpcException(int code, string message) : base(message)
    {
        Code = code;
    }

    public RpcException(int code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
