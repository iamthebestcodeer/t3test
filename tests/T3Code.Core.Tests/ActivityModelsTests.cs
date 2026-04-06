using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class ActivityModelsTests
{
    [Fact]
    public void ActivityId_From_CreatesCorrectly()
    {
        var id = ActivityId.From("act-1");
        Assert.Equal("act-1", id.Value);
        Assert.Equal("act-1", id.ToString());
    }

    [Fact]
    public void ActivityItem_StoresCorrectly()
    {
        var item = new ActivityItem
        {
            Id = ActivityId.From("act-1"),
            Type = "tool",
            Title = "Running linter",
            Detail = "eslint src/",
            Status = ActivityStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
        };

        Assert.Equal("act-1", item.Id.Value);
        Assert.Equal("tool", item.Type);
        Assert.Equal("Running linter", item.Title);
        Assert.Equal("eslint src/", item.Detail);
        Assert.Equal(ActivityStatus.InProgress, item.Status);
    }

    [Fact]
    public void ActivityStatus_AllValues_Covered()
    {
        var values = Enum.GetValues<ActivityStatus>();
        Assert.Equal(5, values.Length);
        Assert.Contains(ActivityStatus.Pending, values);
        Assert.Contains(ActivityStatus.InProgress, values);
        Assert.Contains(ActivityStatus.Completed, values);
        Assert.Contains(ActivityStatus.Failed, values);
        Assert.Contains(ActivityStatus.Cancelled, values);
    }

    [Fact]
    public void ApprovalId_From_CreatesCorrectly()
    {
        var id = ApprovalId.From("appr-1");
        Assert.Equal("appr-1", id.Value);
    }

    [Fact]
    public void ApprovalRequest_Resolve_CreatesNewInstance()
    {
        var request = new ApprovalRequest
        {
            Id = ApprovalId.From("appr-1"),
            Title = "Allow this action?",
            Body = "Run npm install",
            Category = "shell",
            CreatedAt = DateTime.UtcNow,
        };

        var resolved = request.Resolve();

        Assert.False(request.IsResolved);
        Assert.True(resolved.IsResolved);
    }

    [Fact]
    public void ApprovalRequest_Defaults_NotResolved()
    {
        var request = new ApprovalRequest
        {
            Id = ApprovalId.From("appr-1"),
            Title = "Title",
            Body = "Body",
        };

        Assert.False(request.IsResolved);
    }
}
