using T3Code.Core.Abstractions;
using T3Code.Core.Models;

namespace T3Code.Core.State;

public sealed class ConnectionStore : IConnectionStore
{
    private readonly object _lock = new();
    private ConnectionStateSnapshot _state = ConnectionStateSnapshot.Disconnected();
    private ConnectionInfo? _connectionInfo;

    public ConnectionStateSnapshot State
    {
        get { lock (_lock) return _state; }
    }

    public ConnectionInfo? ConnectionInfo
    {
        get { lock (_lock) return _connectionInfo; }
    }

    public event Action<ConnectionStateSnapshot>? StateChanged;

    public void UpdateState(ConnectionStateSnapshot state)
    {
        lock (_lock)
        {
            if (_state == state) return;
            _state = state;
        }
        StateChanged?.Invoke(state);
    }

    public void SetConnectionInfo(ConnectionInfo info)
    {
        lock (_lock)
        {
            _connectionInfo = info;
        }
    }
}

public sealed class OrchestrationStore : IOrchestrationStore
{
    private readonly object _lock = new();
    private OrchestrationSnapshot? _snapshot;

    public OrchestrationSnapshot? Snapshot
    {
        get { lock (_lock) return _snapshot; }
    }

    public event Action<OrchestrationSnapshot>? SnapshotChanged;
    public event Action<OrchestrationEvent>? EventReceived;

    public void ApplySnapshot(OrchestrationSnapshot snapshot)
    {
        lock (_lock)
        {
            _snapshot = snapshot;
        }
        SnapshotChanged?.Invoke(snapshot);
    }

    public void ApplyEvent(OrchestrationEvent evt)
    {
        EventReceived?.Invoke(evt);
    }
}

public sealed class TerminalStore : ITerminalStore
{
    private readonly object _lock = new();
    private readonly List<TerminalSession> _sessions = [];
    private TerminalSession? _activeSession;

    public IReadOnlyList<TerminalSession> Sessions
    {
        get { lock (_lock) return _sessions.ToList().AsReadOnly(); }
    }

    public TerminalSession? ActiveSession
    {
        get { lock (_lock) return _activeSession; }
    }

    public event Action<TerminalEvent>? TerminalEventReceived;
    public event Action<IReadOnlyList<TerminalSession>>? SessionsChanged;

    public void AddSession(TerminalSession session)
    {
        lock (_lock)
        {
            if (_sessions.All(s => s.Id.Value != session.Id.Value))
            {
                _sessions.Add(session);
            }
            if (_activeSession == null)
            {
                _activeSession = session;
            }
        }
        SessionsChanged?.Invoke(Sessions);
    }

    public void RemoveSession(TerminalSessionId sessionId)
    {
        bool changed = false;
        lock (_lock)
        {
            var idx = _sessions.FindIndex(s => s.Id.Value == sessionId.Value);
            if (idx >= 0)
            {
                _sessions.RemoveAt(idx);
                changed = true;
            }
            if (_activeSession?.Id.Value == sessionId.Value)
            {
                _activeSession = _sessions.Count > 0 ? _sessions[0] : null;
            }
        }
        if (changed)
        {
            SessionsChanged?.Invoke(Sessions);
        }
    }

    public void SetActive(TerminalSessionId sessionId)
    {
        lock (_lock)
        {
            _activeSession = _sessions.FirstOrDefault(s => s.Id.Value == sessionId.Value);
        }
    }

    public void HandleEvent(TerminalEvent evt)
    {
        TerminalEventReceived?.Invoke(evt);
    }
}

public sealed class GitStore : IGitStore
{
    private readonly object _lock = new();
    private GitStatus? _status;

    public GitStatus? CurrentStatus
    {
        get { lock (_lock) return _status; }
    }

    public event Action<GitStatus>? StatusUpdated;

    public void UpdateStatus(GitStatus status)
    {
        lock (_lock)
        {
            _status = status;
        }
        StatusUpdated?.Invoke(status);
    }
}

public sealed class SettingsStore : ISettingsStore
{
    private readonly object _lock = new();
    private ServerSettings? _settings;
    private DesktopUpdateState? _updateState;

    public ServerSettings? Settings
    {
        get { lock (_lock) return _settings; }
    }

    public DesktopUpdateState? UpdateState
    {
        get { lock (_lock) return _updateState; }
    }

    public event Action<ServerSettings>? SettingsChanged;
    public event Action<DesktopUpdateState>? UpdateStateChanged;

    public void UpdateSettings(ServerSettings settings)
    {
        lock (_lock)
        {
            _settings = settings;
        }
        SettingsChanged?.Invoke(settings);
    }

    public void UpdateDesktopState(DesktopUpdateState state)
    {
        lock (_lock)
        {
            _updateState = state;
        }
        UpdateStateChanged?.Invoke(state);
    }
}
