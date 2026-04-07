using System.Text.Json.Serialization;

namespace T3Code.Core.Models;

public sealed record OrchestrationEvent
{
    public required long Sequence { get; init; }
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public DateTime Timestamp { get; init; }
}

public sealed record OrchestrationSnapshot
{
    public required long Sequence { get; init; }
    public required IReadOnlyList<Project> Projects { get; init; }
    public required IReadOnlyList<ThreadView> Threads { get; init; }
    public DateTime SnapshotAt { get; init; }
}

public sealed record TurnDiffResult
{
    public required TurnId TurnId { get; init; }
    public required IReadOnlyList<DiffFileEntry> Files { get; init; }
}

public sealed record FullThreadDiffResult
{
    public required ThreadId ThreadId { get; init; }
    public required IReadOnlyList<DiffFileEntry> Files { get; init; }
}

public sealed record DiffFileEntry
{
    public required string Path { get; init; }
    public required string Status { get; init; }
    public string? DiffContent { get; init; }
}

public sealed record ChatMessage
{
    public required string Id { get; init; }
    public required string Role { get; init; }
    public required string Text { get; init; }
    public string? TurnId { get; init; }
    public string? CreatedAt { get; init; }
    public bool Streaming { get; init; }
    public string? CompletedAt { get; init; }
}

public sealed record ThreadView
{
    public required ThreadId Id { get; init; }
    public required ProjectId ProjectId { get; init; }
    public required string Title { get; init; }
    public required ThreadStatus Status { get; init; }
    public string? Branch { get; init; }
    public string? WorktreePath { get; init; }
    public string? CreatedAt { get; init; }
    public string? UpdatedAt { get; init; }
    public IReadOnlyList<ChatMessage> Messages { get; init; } = [];
    public TurnId? CurrentTurnId { get; init; }
}

public sealed record OrchestrationDispatchResult
{
    [JsonPropertyName("sequence")]
    public long Sequence { get; init; }
}
