using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class TerminalModelsTests
{
    [Fact]
    public void TerminalSessionId_From_CreatesCorrectly()
    {
        var id = TerminalSessionId.From("sess-1");
        Assert.Equal("sess-1", id.Value);
        Assert.Equal("sess-1", id.ToString());
    }

    [Fact]
    public void TerminalEvent_DataEvent_CreatesCorrectly()
    {
        var sessionId = TerminalSessionId.From("s1");
        var evt = TerminalEvent.DataEvent(sessionId, "hello output");

        Assert.Equal("s1", evt.SessionId.Value);
        Assert.Equal(TerminalEventType.Data, evt.Type);
        Assert.Equal("hello output", evt.Data);
        Assert.True(evt.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void TerminalEvent_TitleEvent_CreatesCorrectly()
    {
        var sessionId = TerminalSessionId.From("s1");
        var evt = TerminalEvent.TitleEvent(sessionId, "bash - 80x24");

        Assert.Equal(TerminalEventType.Title, evt.Type);
        Assert.Equal("bash - 80x24", evt.Data);
    }

    [Fact]
    public void TerminalEvent_ClosedEvent_CreatesCorrectly()
    {
        var sessionId = TerminalSessionId.From("s1");
        var evt = TerminalEvent.ClosedEvent(sessionId);

        Assert.Equal(TerminalEventType.Closed, evt.Type);
        Assert.Null(evt.Data);
    }

    [Fact]
    public void TerminalEventType_AllValues_Covered()
    {
        var values = Enum.GetValues<TerminalEventType>();
        Assert.Equal(4, values.Length);
        Assert.Contains(TerminalEventType.Data, values);
        Assert.Contains(TerminalEventType.Title, values);
        Assert.Contains(TerminalEventType.Closed, values);
        Assert.Contains(TerminalEventType.Exit, values);
    }

    [Fact]
    public void TerminalResizeRequest_StoresCorrectly()
    {
        var request = new TerminalResizeRequest
        {
            SessionId = TerminalSessionId.From("s1"),
            Cols = 120,
            Rows = 30,
        };

        Assert.Equal("s1", request.SessionId.Value);
        Assert.Equal(120, request.Cols);
        Assert.Equal(30, request.Rows);
    }

    [Fact]
    public void TerminalOpenRequest_StoresCorrectly()
    {
        var request = new TerminalOpenRequest
        {
            SessionId = TerminalSessionId.From("s1"),
            Cwd = "/home/user",
            Cols = 80,
            Rows = 24,
        };

        Assert.Equal("s1", request.SessionId.Value);
        Assert.Equal("/home/user", request.Cwd);
        Assert.Equal(80, request.Cols);
        Assert.Equal(24, request.Rows);
    }
}
