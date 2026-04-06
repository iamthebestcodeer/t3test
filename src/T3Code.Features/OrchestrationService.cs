using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Client;

namespace T3Code.Features;

public sealed class OrchestrationService : IAsyncDisposable
{
    private readonly ITransportClient _transport;
    private readonly IOrchestrationStore _store;
    private ISubscription<object>? _eventSubscription;
    private volatile bool _disposed;

    public OrchestrationService(ITransportClient transport, IOrchestrationStore store)
    {
        _transport = transport;
        _store = store;
    }

    public async Task LoadSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _transport.RequestAsync<OrchestrationSnapshot>(
            "orchestration.getSnapshot",
            new { },
            cancellationToken);

        _store.ApplySnapshot(snapshot);
    }

    public async Task SubscribeToEventsAsync(CancellationToken cancellationToken = default)
    {
        if (_eventSubscription != null)
        {
            await _eventSubscription.DisposeAsync();
        }

        _eventSubscription = await _transport.SubscribeAsync<object>(
            "orchestration.onDomainEvent",
            _ => { },
            cancellationToken: cancellationToken);
    }

    public async Task<TurnDiffResult> GetTurnDiffAsync(
        string turnId,
        CancellationToken cancellationToken = default)
    {
        return await _transport.RequestAsync<TurnDiffResult>(
            "orchestration.getTurnDiff",
            new { turnId },
            cancellationToken);
    }

    public async Task<FullThreadDiffResult> GetFullThreadDiffAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        return await _transport.RequestAsync<FullThreadDiffResult>(
            "orchestration.getFullThreadDiff",
            new { threadId },
            cancellationToken);
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
