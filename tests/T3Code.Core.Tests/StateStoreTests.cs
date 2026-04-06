using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Core.State;

namespace T3Code.Core.Tests;

public class ConnectionStoreTests
{
    [Fact]
    public void InitialState_IsDisconnected()
    {
        var store = new ConnectionStore();

        Assert.Equal(ConnectionState.Disconnected, store.State.State);
        Assert.Null(store.ConnectionInfo);
    }

    [Fact]
    public void UpdateState_ChangesState()
    {
        var store = new ConnectionStore();
        var connected = ConnectionStateSnapshot.Connected();

        store.UpdateState(connected);

        Assert.Equal(ConnectionState.Connected, store.State.State);
    }

    [Fact]
    public void UpdateState_FiresEvent()
    {
        var store = new ConnectionStore();
        ConnectionStateSnapshot? received = null;
        store.StateChanged += s => received = s;

        var state = ConnectionStateSnapshot.Connecting(1);
        store.UpdateState(state);

        Assert.NotNull(received);
        Assert.Equal(ConnectionState.Connecting, received!.State);
    }

    [Fact]
    public void UpdateState_SameValue_DoesNotFireEvent()
    {
        var store = new ConnectionStore();
        var state = ConnectionStateSnapshot.Disconnected();
        int callCount = 0;
        store.StateChanged += _ => callCount++;

        store.UpdateState(state);

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void SetConnectionInfo_StoresValue()
    {
        var store = new ConnectionStore();
        var info = new ConnectionInfo
        {
            WebSocketUrl = "ws://localhost:8080",
            Port = 8080,
            AuthToken = "abc",
        };

        store.SetConnectionInfo(info);

        Assert.NotNull(store.ConnectionInfo);
        Assert.Equal("ws://localhost:8080", store.ConnectionInfo!.WebSocketUrl);
        Assert.Equal(8080, store.ConnectionInfo.Port);
    }

    [Fact]
    public void State_ReturnsThreadSafeCopy()
    {
        var store = new ConnectionStore();
        store.UpdateState(ConnectionStateSnapshot.Connected());

        var state1 = store.State;
        var state2 = store.State;

        Assert.Equal(state1.State, state2.State);
    }
}

public class OrchestrationStoreTests
{
    [Fact]
    public void InitialState_IsNull()
    {
        var store = new OrchestrationStore();

        Assert.Null(store.Snapshot);
    }

    [Fact]
    public void ApplySnapshot_StoresValue()
    {
        var store = new OrchestrationStore();
        var snapshot = new OrchestrationSnapshot
        {
            Sequence = 1,
            Projects = [],
            Threads = [],
            SnapshotAt = DateTime.UtcNow,
        };

        store.ApplySnapshot(snapshot);

        Assert.NotNull(store.Snapshot);
        Assert.Equal(1, store.Snapshot!.Sequence);
    }

    [Fact]
    public void ApplySnapshot_FiresEvent()
    {
        var store = new OrchestrationStore();
        OrchestrationSnapshot? received = null;
        store.SnapshotChanged += s => received = s;

        var snapshot = new OrchestrationSnapshot
        {
            Sequence = 1,
            Projects = [],
            Threads = [],
            SnapshotAt = DateTime.UtcNow,
        };
        store.ApplySnapshot(snapshot);

        Assert.NotNull(received);
        Assert.Equal(1, received!.Sequence);
    }

    [Fact]
    public void ApplyEvent_FiresEvent()
    {
        var store = new OrchestrationStore();
        OrchestrationEvent? received = null;
        store.EventReceived += e => received = e;

        var evt = new OrchestrationEvent
        {
            Sequence = 42,
            Type = "thread.created",
            Payload = "{}",
        };
        store.ApplyEvent(evt);

        Assert.NotNull(received);
        Assert.Equal(42, received!.Sequence);
        Assert.Equal("thread.created", received.Type);
    }
}

public class TerminalStoreTests
{
    [Fact]
    public void InitialState_EmptySessions()
    {
        var store = new TerminalStore();

        Assert.Empty(store.Sessions);
        Assert.Null(store.ActiveSession);
    }

    [Fact]
    public void AddSession_AddsToList()
    {
        var store = new TerminalStore();
        var session = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"),
            Cols = 80,
            Rows = 24,
            Title = "bash",
            CreatedAt = DateTime.UtcNow,
        };

        store.AddSession(session);

        Assert.Single(store.Sessions);
        Assert.Equal("s1", store.Sessions[0].Id.Value);
    }

    [Fact]
    public void AddSession_SetsFirstAsActive()
    {
        var store = new TerminalStore();
        var session = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"),
            Cols = 80,
            Rows = 24,
            Title = "bash",
            CreatedAt = DateTime.UtcNow,
        };

        store.AddSession(session);

        Assert.NotNull(store.ActiveSession);
        Assert.Equal("s1", store.ActiveSession!.Id.Value);
    }

    [Fact]
    public void AddSession_Duplicate_DoesNotAddTwice()
    {
        var store = new TerminalStore();
        var session = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"),
            Cols = 80,
            Rows = 24,
            Title = "bash",
            CreatedAt = DateTime.UtcNow,
        };

        store.AddSession(session);
        store.AddSession(session);

        Assert.Single(store.Sessions);
    }

    [Fact]
    public void RemoveSession_RemovesFromList()
    {
        var store = new TerminalStore();
        var session = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"),
            Cols = 80,
            Rows = 24,
            Title = "bash",
            CreatedAt = DateTime.UtcNow,
        };

        store.AddSession(session);
        store.RemoveSession(TerminalSessionId.From("s1"));

        Assert.Empty(store.Sessions);
        Assert.Null(store.ActiveSession);
    }

    [Fact]
    public void RemoveSession_FiresSessionsChanged()
    {
        var store = new TerminalStore();
        var session = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"),
            Cols = 80,
            Rows = 24,
            Title = "bash",
            CreatedAt = DateTime.UtcNow,
        };
        store.AddSession(session);

        int eventCount = 0;
        store.SessionsChanged += _ => eventCount++;
        store.RemoveSession(TerminalSessionId.From("s1"));

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void SetActive_ChangesActiveSession()
    {
        var store = new TerminalStore();
        var s1 = new TerminalSession
        {
            Id = TerminalSessionId.From("s1"), Cols = 80, Rows = 24,
            Title = "bash", CreatedAt = DateTime.UtcNow,
        };
        var s2 = new TerminalSession
        {
            Id = TerminalSessionId.From("s2"), Cols = 80, Rows = 24,
            Title = "zsh", CreatedAt = DateTime.UtcNow,
        };

        store.AddSession(s1);
        store.AddSession(s2);
        store.SetActive(TerminalSessionId.From("s2"));

        Assert.Equal("s2", store.ActiveSession?.Id.Value);
    }

    [Fact]
    public void HandleEvent_FiresEvent()
    {
        var store = new TerminalStore();
        TerminalEvent? received = null;
        store.TerminalEventReceived += e => received = e;

        var evt = TerminalEvent.DataEvent(TerminalSessionId.From("s1"), "hello");
        store.HandleEvent(evt);

        Assert.NotNull(received);
        Assert.Equal("hello", received!.Data);
    }

    [Fact]
    public void RemoveSession_NonExistent_NoChange()
    {
        var store = new TerminalStore();

        // Should not throw
        store.RemoveSession(TerminalSessionId.From("nonexistent"));

        Assert.Empty(store.Sessions);
    }
}

public class GitStoreTests
{
    [Fact]
    public void InitialState_IsNull()
    {
        var store = new GitStore();

        Assert.Null(store.CurrentStatus);
    }

    [Fact]
    public void UpdateStatus_StoresValue()
    {
        var store = new GitStore();
        var status = GitStatus.Empty("/repo");

        store.UpdateStatus(status);

        Assert.NotNull(store.CurrentStatus);
        Assert.Equal("/repo", store.CurrentStatus!.Cwd);
    }

    [Fact]
    public void UpdateStatus_FiresEvent()
    {
        var store = new GitStore();
        GitStatus? received = null;
        store.StatusUpdated += s => received = s;

        var status = GitStatus.Empty("/repo");
        store.UpdateStatus(status);

        Assert.NotNull(received);
        Assert.Equal("/repo", received!.Cwd);
    }
}

public class SettingsStoreTests
{
    [Fact]
    public void InitialState_IsNull()
    {
        var store = new SettingsStore();

        Assert.Null(store.Settings);
        Assert.Null(store.UpdateState);
    }

    [Fact]
    public void UpdateSettings_StoresValue()
    {
        var store = new SettingsStore();
        var settings = new ServerSettings { DefaultRuntimeMode = "desktop" };

        store.UpdateSettings(settings);

        Assert.NotNull(store.Settings);
        Assert.Equal("desktop", store.Settings!.DefaultRuntimeMode);
    }

    [Fact]
    public void UpdateSettings_FiresEvent()
    {
        var store = new SettingsStore();
        ServerSettings? received = null;
        store.SettingsChanged += s => received = s;

        var settings = new ServerSettings { DefaultRuntimeMode = "desktop" };
        store.UpdateSettings(settings);

        Assert.NotNull(received);
        Assert.Equal("desktop", received!.DefaultRuntimeMode);
    }

    [Fact]
    public void UpdateDesktopState_StoresValue()
    {
        var store = new SettingsStore();
        var state = DesktopUpdateState.Disabled("1.0.0");

        store.UpdateDesktopState(state);

        Assert.NotNull(store.UpdateState);
        Assert.Equal("1.0.0", store.UpdateState!.CurrentVersion);
    }

    [Fact]
    public void UpdateDesktopState_FiresEvent()
    {
        var store = new SettingsStore();
        DesktopUpdateState? received = null;
        store.UpdateStateChanged += s => received = s;

        var state = DesktopUpdateState.Disabled("1.0.0");
        store.UpdateDesktopState(state);

        Assert.NotNull(received);
        Assert.Equal(UpdateStatus.Disabled, received!.Status);
    }
}
