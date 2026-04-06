namespace T3Code.Core.Models;

public sealed record TerminalSessionId
{
    public required string Value { get; init; }

    public static TerminalSessionId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public sealed record TerminalSession
{
    public required TerminalSessionId Id { get; init; }
    public required int Cols { get; init; }
    public required int Rows { get; init; }
    public required string Title { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record TerminalEvent
{
    public required TerminalSessionId SessionId { get; init; }
    public required TerminalEventType Type { get; init; }
    public string? Data { get; init; }
    public DateTime Timestamp { get; init; }

    public static TerminalEvent DataEvent(TerminalSessionId sessionId, string data) => new()
    {
        SessionId = sessionId,
        Type = TerminalEventType.Data,
        Data = data,
        Timestamp = DateTime.UtcNow,
    };

    public static TerminalEvent TitleEvent(TerminalSessionId sessionId, string title) => new()
    {
        SessionId = sessionId,
        Type = TerminalEventType.Title,
        Data = title,
        Timestamp = DateTime.UtcNow,
    };

    public static TerminalEvent ClosedEvent(TerminalSessionId sessionId) => new()
    {
        SessionId = sessionId,
        Type = TerminalEventType.Closed,
        Timestamp = DateTime.UtcNow,
    };
}

public enum TerminalEventType
{
    Data,
    Title,
    Closed,
    Exit,
}

public sealed record TerminalResizeRequest
{
    public required TerminalSessionId SessionId { get; init; }
    public required int Cols { get; init; }
    public required int Rows { get; init; }
}

public sealed record TerminalOpenRequest
{
    public required TerminalSessionId SessionId { get; init; }
    public required string Cwd { get; init; }
    public required int Cols { get; init; }
    public required int Rows { get; init; }
}
