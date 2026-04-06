using T3Code.Core.Models;

namespace T3Code.Core.Abstractions;

public interface ITransportClient : IAsyncDisposable
{
    ConnectionStateSnapshot ConnectionState { get; }
    event Action<ConnectionStateSnapshot>? ConnectionStateChanged;

    Task ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    Task<TResponse> RequestAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TResponse>> RequestListAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<ISubscription<TItem>> SubscribeAsync<TItem>(
        string method,
        Action<TItem> handler,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}

public interface ISubscription<T> : IAsyncDisposable
{
    bool IsActive { get; }
}
