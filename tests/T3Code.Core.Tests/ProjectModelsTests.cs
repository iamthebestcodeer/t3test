using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class ProjectModelsTests
{
    [Fact]
    public void ProjectId_From_CreatesCorrectly()
    {
        var id = ProjectId.From("proj-123");

        Assert.Equal("proj-123", id.Value);
    }

    [Fact]
    public void ProjectId_ToString_ReturnsValue()
    {
        var id = ProjectId.From("test-proj");

        Assert.Equal("test-proj", id.ToString());
    }

    [Fact]
    public void Project_Create_SetsDefaults()
    {
        var project = Project.Create("id-1", "My Project", "/path/to/project");

        Assert.Equal("id-1", project.Id.Value);
        Assert.Equal("My Project", project.Title);
        Assert.Equal("/path/to/project", project.Cwd);
        Assert.True(project.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void ProjectId_Equality_WithSameValue()
    {
        var id1 = ProjectId.From("proj-1");
        var id2 = ProjectId.From("proj-1");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Project_Equality_WithSameValues()
    {
        var now = DateTime.UtcNow;
        var p1 = new Project
        {
            Id = ProjectId.From("id-1"),
            Title = "Test",
            Cwd = "/tmp",
            CreatedAt = now,
        };
        var p2 = new Project
        {
            Id = ProjectId.From("id-1"),
            Title = "Test",
            Cwd = "/tmp",
            CreatedAt = now,
        };

        Assert.Equal(p1, p2);
    }
}
