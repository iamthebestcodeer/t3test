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
    public required IReadOnlyList<Thread> Threads { get; init; }
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
