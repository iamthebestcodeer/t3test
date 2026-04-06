namespace T3Code.Core.Models;

public sealed record ProjectId
{
    public required string Value { get; init; }

    public static ProjectId From(string value) => new() { Value = value };

    public override string ToString() => Value;
}

public sealed record Project
{
    public required ProjectId Id { get; init; }
    public required string Title { get; init; }
    public required string Cwd { get; init; }
    public DateTime CreatedAt { get; init; }

    public static Project Create(string id, string title, string cwd) => new()
    {
        Id = ProjectId.From(id),
        Title = title,
        Cwd = cwd,
        CreatedAt = DateTime.UtcNow,
    };
}
