using T3Code.Core.Models;

namespace T3Code.Core.Abstractions;

public interface IOrchestrationStore
{
    OrchestrationSnapshot? Snapshot { get; }
    event Action<OrchestrationSnapshot>? SnapshotChanged;
    event Action<OrchestrationEvent>? EventReceived;

    void ApplySnapshot(OrchestrationSnapshot snapshot);
    void ApplyEvent(OrchestrationEvent evt);
}

public interface ITerminalStore
{
    IReadOnlyList<TerminalSession> Sessions { get; }
    TerminalSession? ActiveSession { get; }
    event Action<TerminalEvent>? TerminalEventReceived;
    event Action<IReadOnlyList<TerminalSession>>? SessionsChanged;

    void AddSession(TerminalSession session);
    void RemoveSession(TerminalSessionId sessionId);
    void SetActive(TerminalSessionId sessionId);
    void HandleEvent(TerminalEvent evt);
}

public interface IGitStore
{
    GitStatus? CurrentStatus { get; }
    event Action<GitStatus>? StatusUpdated;

    void UpdateStatus(GitStatus status);
}

public interface ISettingsStore
{
    ServerSettings? Settings { get; }
    DesktopUpdateState? UpdateState { get; }
    event Action<ServerSettings>? SettingsChanged;
    event Action<DesktopUpdateState>? UpdateStateChanged;

    void UpdateSettings(ServerSettings settings);
    void UpdateDesktopState(DesktopUpdateState state);
}
