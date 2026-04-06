using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class ThreadModelsTests
{
    [Fact]
    public void ThreadId_From_CreatesCorrectly()
    {
        var id = ThreadId.From("thread-456");

        Assert.Equal("thread-456", id.Value);
        Assert.Equal("thread-456", id.ToString());
    }

    [Fact]
    public void Turn_CreateUser_SetsFields()
    {
        var turn = Turn.CreateUser("turn-1", "Hello, world!");

        Assert.Equal("turn-1", turn.Id.Value);
        Assert.Equal(TurnRole.User, turn.Role);
        Assert.Equal("Hello, world!", turn.Content);
        Assert.True(turn.CreatedAt <= DateTime.UtcNow);
        Assert.False(turn.IsStreaming);
    }

    [Fact]
    public void Turn_CreateAssistant_SetsFields()
    {
        var turn = Turn.CreateAssistant("turn-2", "Hi there!", streaming: true);

        Assert.Equal("turn-2", turn.Id.Value);
        Assert.Equal(TurnRole.Assistant, turn.Role);
        Assert.Equal("Hi there!", turn.Content);
        Assert.True(turn.IsStreaming);
    }

    [Fact]
    public void Thread_WithStatus_CreatesNewInstance()
    {
        var thread = new Models.Thread
        {
            Id = ThreadId.From("t1"),
            ProjectId = ProjectId.From("p1"),
            Title = "Thread 1",
            Status = ThreadStatus.Idle,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var updated = thread.WithStatus(ThreadStatus.Running);

        Assert.Equal(ThreadStatus.Idle, thread.Status);
        Assert.Equal(ThreadStatus.Running, updated.Status);
        Assert.True(updated.UpdatedAt >= thread.UpdatedAt);
    }

    [Fact]
    public void Thread_WithCurrentTurn_CreatesNewInstance()
    {
        var thread = new Models.Thread
        {
            Id = ThreadId.From("t1"),
            ProjectId = ProjectId.From("p1"),
            Title = "Thread 1",
            Status = ThreadStatus.Running,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var turnId = TurnId.From("turn-3");
        var updated = thread.WithCurrentTurn(turnId);

        Assert.Null(thread.CurrentTurnId);
        Assert.Equal("turn-3", updated.CurrentTurnId?.Value);
    }

    [Fact]
    public void ThreadStatus_AllValues_Covered()
    {
        var values = Enum.GetValues<ThreadStatus>();
        Assert.Equal(7, values.Length);
        Assert.Contains(ThreadStatus.Idle, values);
        Assert.Contains(ThreadStatus.Starting, values);
        Assert.Contains(ThreadStatus.Running, values);
        Assert.Contains(ThreadStatus.Interrupted, values);
        Assert.Contains(ThreadStatus.Ready, values);
        Assert.Contains(ThreadStatus.Stopped, values);
        Assert.Contains(ThreadStatus.Error, values);
    }

    [Fact]
    public void TurnRole_AllValues_Covered()
    {
        var values = Enum.GetValues<TurnRole>();
        Assert.Equal(3, values.Length);
        Assert.Contains(TurnRole.User, values);
        Assert.Contains(TurnRole.Assistant, values);
        Assert.Contains(TurnRole.System, values);
    }
}
