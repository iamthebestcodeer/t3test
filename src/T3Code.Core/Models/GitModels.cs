namespace T3Code.Core.Models;

public sealed record GitBranch
{
    public required string Name { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsRemote { get; init; }
    public string? Upstream { get; init; }
    public DateTime? LastCommitDate { get; init; }
}

public sealed record GitStatusFile
{
    public required string Path { get; init; }
    public required GitFileStatus Status { get; init; }
    public bool Staged { get; init; }
}

public enum GitFileStatus
{
    Modified,
    Added,
    Deleted,
    Renamed,
    Copied,
    Untracked,
    Ignored,
    Unmerged,
}

public sealed record GitStatus
{
    public required string Branch { get; init; }
    public required string Cwd { get; init; }
    public required IReadOnlyList<GitStatusFile> Files { get; init; }
    public int Ahead { get; init; }
    public int Behind { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static GitStatus Empty(string cwd) => new()
    {
        Branch = string.Empty,
        Cwd = cwd,
        Files = [],
        UpdatedAt = DateTime.UtcNow,
    };
}

public sealed record GitWorktree
{
    public required string Path { get; init; }
    public required string Branch { get; init; }
    public required string Head { get; init; }
}

public sealed record PullRequestRef
{
    public required string Number { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string HeadRef { get; init; }
    public required string BaseRef { get; init; }
    public string? State { get; init; }
}
