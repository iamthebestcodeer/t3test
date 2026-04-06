using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Client;

namespace T3Code.Features;

public sealed class TerminalService : IAsyncDisposable
{
    private readonly ITransportClient _transport;
    private readonly ITerminalStore _store;
    private ISubscription<object>? _eventSubscription;
    private volatile bool _disposed;

    public TerminalService(ITransportClient transport, ITerminalStore store)
    {
        _transport = transport;
        _store = store;
    }

    public async Task<TerminalSession> OpenAsync(
        string cwd,
        int cols,
        int rows,
        CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var request = new TerminalOpenRequest
        {
            SessionId = TerminalSessionId.From(sessionId),
            Cwd = cwd,
            Cols = cols,
            Rows = rows,
        };

        var session = await _transport.RequestAsync<TerminalSession>(
            "terminal.open",
            request,
            cancellationToken);

        _store.AddSession(session);
        return session;
    }

    public async Task WriteAsync(
        TerminalSessionId sessionId,
        string data,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "terminal.write",
            new { sessionId = sessionId.Value, data },
            cancellationToken);
    }

    public async Task ResizeAsync(
        TerminalSessionId sessionId,
        int cols,
        int rows,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "terminal.resize",
            new { sessionId = sessionId.Value, cols, rows },
            cancellationToken);
    }

    public async Task CloseAsync(
        TerminalSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "terminal.close",
            new { sessionId = sessionId.Value },
            cancellationToken);

        _store.RemoveSession(sessionId);
    }

    public async Task ClearAsync(
        TerminalSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "terminal.clear",
            new { sessionId = sessionId.Value },
            cancellationToken);
    }

    public async Task<TerminalSession> RestartAsync(
        TerminalSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _transport.RequestAsync<TerminalSession>(
            "terminal.restart",
            new { sessionId = sessionId.Value },
            cancellationToken);

        _store.RemoveSession(sessionId);
        _store.AddSession(session);
        return session;
    }

    public async Task SubscribeToEventsAsync(CancellationToken cancellationToken = default)
    {
        if (_eventSubscription != null)
        {
            await _eventSubscription.DisposeAsync();
        }

        _eventSubscription = await _transport.SubscribeAsync<object>(
            "terminal.onEvent",
            _ => { },
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_eventSubscription != null)
        {
            await _eventSubscription.DisposeAsync();
        }
    }
}
