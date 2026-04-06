namespace T3Code.Core.Models;

public sealed record ConnectionInfo
{
    public required string WebSocketUrl { get; init; }
    public required int Port { get; init; }
    public required string AuthToken { get; init; }
    public string? T3Home { get; init; }
}

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed,
}

public sealed record ConnectionStateSnapshot
{
    public required ConnectionState State { get; init; }
    public int ReconnectAttempt { get; init; }
    public string? LastError { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public DateTime? DisconnectedAt { get; init; }

    public static ConnectionStateSnapshot Disconnected() => new()
    {
        State = ConnectionState.Disconnected,
    };

    public static ConnectionStateSnapshot Connecting(int attempt) => new()
    {
        State = ConnectionState.Connecting,
        ReconnectAttempt = attempt,
    };

    public static ConnectionStateSnapshot Connected() => new()
    {
        State = ConnectionState.Connected,
        ConnectedAt = DateTime.UtcNow,
    };

    public static ConnectionStateSnapshot Reconnecting(int attempt, string? error = null) => new()
    {
        State = ConnectionState.Reconnecting,
        ReconnectAttempt = attempt,
        LastError = error,
    };

    public static ConnectionStateSnapshot Failed(string error) => new()
    {
        State = ConnectionState.Failed,
        LastError = error,
        DisconnectedAt = DateTime.UtcNow,
    };
}
