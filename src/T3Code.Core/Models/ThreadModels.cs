namespace T3Code.Core.Models;

public sealed record ThreadId
{
    public required string Value { get; init; }

    public static ThreadId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public enum ThreadStatus
{
    Idle,
    Starting,
    Running,
    Interrupted,
    Ready,
    Stopped,
    Error,
}

public enum TurnRole
{
    User,
    Assistant,
    System,
}

public sealed record TurnId
{
    public required string Value { get; init; }

    public static TurnId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public sealed record Turn
{
    public required TurnId Id { get; init; }
    public required TurnRole Role { get; init; }
    public required string Content { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsStreaming { get; init; }

    public static Turn CreateUser(string id, string content) => new()
    {
        Id = TurnId.From(id),
        Role = TurnRole.User,
        Content = content,
        CreatedAt = DateTime.UtcNow,
    };

    public static Turn CreateAssistant(string id, string content, bool streaming = false) => new()
    {
        Id = TurnId.From(id),
        Role = TurnRole.Assistant,
        Content = content,
        CreatedAt = DateTime.UtcNow,
        IsStreaming = streaming,
    };
}

public sealed record Thread
{
    public required ThreadId Id { get; init; }
    public required ProjectId ProjectId { get; init; }
    public required string Title { get; init; }
    public required ThreadStatus Status { get; init; }
    public TurnId? CurrentTurnId { get; init; }
    public string? Branch { get; init; }
    public string? WorktreePath { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public Thread WithStatus(ThreadStatus status) => this with
    {
        Status = status,
        UpdatedAt = DateTime.UtcNow,
    };

    public Thread WithCurrentTurn(TurnId? turnId) => this with
    {
        CurrentTurnId = turnId,
        UpdatedAt = DateTime.UtcNow,
    };
}
