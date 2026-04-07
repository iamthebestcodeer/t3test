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
            evt =>
            {
                try
                {
                    if (evt is System.Text.Json.JsonElement elem && elem.TryGetProperty("type", out var typeProp))
                    {
                        var type = typeProp.GetString();
                        var seq = 0L;
                        if (elem.TryGetProperty("sequence", out var seqProp))
                        {
                            seq = seqProp.GetInt64();
                        }
                        var payload = elem.GetRawText();

                        _store.ApplyEvent(new OrchestrationEvent
                        {
                            Sequence = seq,
                            Type = type ?? "unknown",
                            Payload = payload,
                            Timestamp = DateTime.UtcNow,
                        });
                    }
                }
                catch
                {
                    // Swallow deserialization errors for individual events
                }
            },
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

    public async Task<OrchestrationDispatchResult> DispatchCreateThreadAsync(
        string projectId,
        string threadId,
        string title,
        string? branch = null,
        string? worktreePath = null,
        string? runtimeMode = null,
        string? interactionMode = null,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:thread.create:{Guid.NewGuid()}";
        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.create",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["projectId"] = projectId,
            ["title"] = title,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
        };

        if (branch != null) command["branch"] = branch;
        if (worktreePath != null) command["worktreePath"] = worktreePath;
        if (runtimeMode != null) command["runtimeMode"] = runtimeMode;
        if (interactionMode != null) command["interactionMode"] = interactionMode;

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
            cancellationToken);
    }

    public async Task<OrchestrationDispatchResult> DispatchTurnStartAsync(
        string threadId,
        string prompt,
        string? turnId = null,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:turn.start:{Guid.NewGuid()}";
        var resolvedTurnId = turnId ?? Guid.NewGuid().ToString();

        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.turn.start",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["turnId"] = resolvedTurnId,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
            ["prompt"] = prompt,
        };

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
            cancellationToken);
    }

    public async Task<OrchestrationDispatchResult> DispatchTurnInterruptAsync(
        string threadId,
        string? turnId = null,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:turn.interrupt:{Guid.NewGuid()}";
        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.turn.interrupt",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
        };

        if (turnId != null) command["turnId"] = turnId;

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
            cancellationToken);
    }

    public async Task<OrchestrationDispatchResult> DispatchArchiveThreadAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:thread.archive:{Guid.NewGuid()}";
        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.archive",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
        };

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
            cancellationToken);
    }

    public async Task<OrchestrationDispatchResult> DispatchRevertThreadAsync(
        string threadId,
        int turnCount,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:thread.revert:{Guid.NewGuid()}";
        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.revert",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["turnCount"] = turnCount,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
        };

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
            cancellationToken);
    }

    public async Task<OrchestrationDispatchResult> DispatchApprovalResponseAsync(
        string threadId,
        string approvalId,
        bool approved,
        string? body = null,
        CancellationToken cancellationToken = default)
    {
        var commandId = $"desktop:approval.response:{Guid.NewGuid()}";
        var command = new Dictionary<string, object>
        {
            ["type"] = "thread.activity.append",
            ["commandId"] = commandId,
            ["threadId"] = threadId,
            ["createdAt"] = DateTime.UtcNow.ToString("o"),
            ["activity"] = new Dictionary<string, object>
            {
                ["id"] = approvalId,
                ["kind"] = approved ? "approval.granted" : "approval.denied",
                ["tone"] = approved ? "info" : "warning",
                ["summary"] = approved ? "Approved" : "Denied",
                ["turnId"] = string.Empty,
                ["createdAt"] = DateTime.UtcNow.ToString("o"),
            },
        };

        if (body != null)
        {
            var activityDict = (Dictionary<string, object>)command["activity"];
            activityDict["payload"] = new { body };
        }

        return await _transport.RequestAsync<OrchestrationDispatchResult>(
            "orchestration.dispatchCommand",
            command,
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
