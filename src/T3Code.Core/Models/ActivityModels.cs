namespace T3Code.Core.Models;

public sealed record ActivityId
{
    public required string Value { get; init; }

    public static ActivityId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public enum ActivityStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
}

public sealed record ActivityItem
{
    public required ActivityId Id { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public string? Detail { get; init; }
    public ActivityStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record ApprovalId
{
    public required string Value { get; init; }

    public static ApprovalId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public sealed record ApprovalRequest
{
    public required ApprovalId Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public string? Category { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsResolved { get; init; }

    public ApprovalRequest Resolve() => this with { IsResolved = true };
}
