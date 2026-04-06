using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class ConnectionModelsTests
{
    [Fact]
    public void Disconnected_SetsCorrectState()
    {
        var state = ConnectionStateSnapshot.Disconnected();

        Assert.Equal(ConnectionState.Disconnected, state.State);
        Assert.Equal(0, state.ReconnectAttempt);
        Assert.Null(state.LastError);
        Assert.Null(state.ConnectedAt);
        Assert.Null(state.DisconnectedAt);
    }

    [Fact]
    public void Connecting_SetsAttemptNumber()
    {
        var state = ConnectionStateSnapshot.Connecting(3);

        Assert.Equal(ConnectionState.Connecting, state.State);
        Assert.Equal(3, state.ReconnectAttempt);
    }

    [Fact]
    public void Connected_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        var state = ConnectionStateSnapshot.Connected();
        var after = DateTime.UtcNow;

        Assert.Equal(ConnectionState.Connected, state.State);
        Assert.NotNull(state.ConnectedAt);
        Assert.True(state.ConnectedAt >= before);
        Assert.True(state.ConnectedAt <= after);
    }

    [Fact]
    public void Reconnecting_WithOptionalError()
    {
        var state = ConnectionStateSnapshot.Reconnecting(5, "timeout");

        Assert.Equal(ConnectionState.Reconnecting, state.State);
        Assert.Equal(5, state.ReconnectAttempt);
        Assert.Equal("timeout", state.LastError);
    }

    [Fact]
    public void Failed_SetsErrorAndTimestamp()
    {
        var before = DateTime.UtcNow;
        var state = ConnectionStateSnapshot.Failed("connection refused");
        var after = DateTime.UtcNow;

        Assert.Equal(ConnectionState.Failed, state.State);
        Assert.Equal("connection refused", state.LastError);
        Assert.NotNull(state.DisconnectedAt);
        Assert.True(state.DisconnectedAt >= before);
        Assert.True(state.DisconnectedAt <= after);
    }

    [Fact]
    public void ConnectionInfo_StoresCorrectValues()
    {
        var info = new ConnectionInfo
        {
            WebSocketUrl = "ws://127.0.0.1:9222/?token=abc",
            Port = 9222,
            AuthToken = "abc",
            T3Home = "/tmp/t3",
        };

        Assert.Equal("ws://127.0.0.1:9222/?token=abc", info.WebSocketUrl);
        Assert.Equal(9222, info.Port);
        Assert.Equal("abc", info.AuthToken);
        Assert.Equal("/tmp/t3", info.T3Home);
    }

    [Fact]
    public void ConnectionInfo_T3Home_Optional()
    {
        var info = new ConnectionInfo
        {
            WebSocketUrl = "ws://localhost:8080",
            Port = 8080,
            AuthToken = "token",
        };

        Assert.Null(info.T3Home);
    }

    [Fact]
    public void ConnectionState_AllValues_Covered()
    {
        var values = Enum.GetValues<ConnectionState>();
        Assert.Equal(5, values.Length);
        Assert.Contains(ConnectionState.Disconnected, values);
        Assert.Contains(ConnectionState.Connecting, values);
        Assert.Contains(ConnectionState.Connected, values);
        Assert.Contains(ConnectionState.Reconnecting, values);
        Assert.Contains(ConnectionState.Failed, values);
    }
}
