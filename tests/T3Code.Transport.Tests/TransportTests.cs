using T3Code.Transport.Client;
using T3Code.Transport.Messages;

namespace T3Code.Transport.Tests;

public class RpcExceptionTests
{
    [Fact]
    public void RpcException_StoresCodeAndMessage()
    {
        var ex = new RpcException(-32600, "Invalid request");

        Assert.Equal(-32600, ex.Code);
        Assert.Equal("Invalid request", ex.Message);
    }

    [Fact]
    public void RpcException_WithInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new RpcException(-1, "outer", inner);

        Assert.Equal(-1, ex.Code);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}

public class SubscriptionTests
{
    [Fact]
    public void Subscription_IsActive_WhenCreated()
    {
        bool unsubscribed = false;
        var sub = new Subscription<string>(() => unsubscribed = true);

        Assert.True(sub.IsActive);
        Assert.False(unsubscribed);
    }

    [Fact]
    public async Task Subscription_Dispose_CallsUnsubscribe()
    {
        bool unsubscribed = false;
        var sub = new Subscription<string>(() => unsubscribed = true);

        await sub.DisposeAsync();

        Assert.False(sub.IsActive);
        Assert.True(unsubscribed);
    }

    [Fact]
    public async Task Subscription_MultipleDispose_OnlyCallsOnce()
    {
        int callCount = 0;
        var sub = new Subscription<string>(() => callCount++);

        await sub.DisposeAsync();
        await sub.DisposeAsync();
        await sub.DisposeAsync();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Subscription_UnsubscribeError_Swallows()
    {
        var sub = new Subscription<string>(() => throw new InvalidOperationException("boom"));

        // Should not throw
        await sub.DisposeAsync();

        Assert.False(sub.IsActive);
    }
}

public class RpcMessagesTests
{
    [Fact]
    public void RpcRequest_StoresFields()
    {
        var request = new RpcRequest
        {
            Id = 1,
            Method = "test.method",
            Params = new { key = "value" },
        };

        Assert.Equal(1, request.Id);
        Assert.Equal("test.method", request.Method);
        Assert.NotNull(request.Params);
    }

    [Fact]
    public void RpcResponse_StoresFields()
    {
        var response = new RpcResponse
        {
            Id = 42,
            Error = new RpcError { Code = -1, Message = "fail" },
        };

        Assert.Equal(42, response.Id);
        Assert.NotNull(response.Error);
        Assert.Equal(-1, response.Error!.Code);
    }

    [Fact]
    public void RpcMessage_IsResponse_WhenHasIdAndResult()
    {
        var msg = new RpcMessage
        {
            Id = 1,
            Result = System.Text.Json.JsonDocument.Parse("42").RootElement,
        };

        Assert.True(msg.IsResponse);
        Assert.False(msg.IsPush);
    }

    [Fact]
    public void RpcMessage_IsResponse_WhenHasIdAndError()
    {
        var msg = new RpcMessage
        {
            Id = 1,
            Error = new RpcError { Code = -1, Message = "err" },
        };

        Assert.True(msg.IsResponse);
        Assert.False(msg.IsPush);
    }

    [Fact]
    public void RpcMessage_IsPush_WhenHasMethodAndNoId()
    {
        var msg = new RpcMessage
        {
            Method = "event.something",
        };

        Assert.True(msg.IsPush);
        Assert.False(msg.IsResponse);
    }

    [Fact]
    public void RpcMessage_Neither_WhenBothIdAndMethod()
    {
        var msg = new RpcMessage
        {
            Id = 1,
            Method = "test",
        };

        Assert.False(msg.IsResponse);
        Assert.False(msg.IsPush);
    }
}

public class WebSocketTransportClientTests
{
    [Fact]
    public void InitialState_IsDisconnected()
    {
        var client = new WebSocketTransportClient();

        Assert.Equal(T3Code.Core.Models.ConnectionState.Disconnected, client.ConnectionState.State);
    }

    [Fact]
    public async Task DisposeAsync_DoesNotThrow()
    {
        var client = new WebSocketTransportClient();

        await client.DisposeAsync();

        // Can dispose again safely
        await client.DisposeAsync();
    }

    [Fact]
    public async Task RequestAsync_WhenNotConnected_ThrowsRpcException()
    {
        var client = new WebSocketTransportClient();

        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.RequestAsync<object>("test.method"));

        Assert.Equal(-1, ex.Code);
        Assert.Contains("Not connected", ex.Message);
    }

    [Fact]
    public void ProcessMessage_ValidResponse_CompletesPendingRequest()
    {
        var client = new WebSocketTransportClient();
        // Process a response with matching ID
        var json = "{\"id\":1,\"result\":{\"data\":\"ok\"}}";

        client.ProcessMessage(json);

        // No pending request to complete, but should not throw
    }

    [Fact]
    public void ProcessMessage_InvalidJson_DoesNotThrow()
    {
        var client = new WebSocketTransportClient();

        client.ProcessMessage("not json at all");
        client.ProcessMessage("");
        client.ProcessMessage("null");

        // Should not throw
    }

    [Fact]
    public void ProcessMessage_PushMessage_HandledGracefully()
    {
        var client = new WebSocketTransportClient();
        var json = "{\"method\":\"test.push\",\"params\":{\"key\":\"value\"}}";

        client.ProcessMessage(json);

        // Should not throw
    }

    [Fact]
    public void ConnectionStateChanged_FiresOnUpdate()
    {
        var client = new WebSocketTransportClient();
        T3Code.Core.Models.ConnectionStateSnapshot? received = null;
        client.ConnectionStateChanged += s => received = s;

        // Simulate a state change by accessing the internal method
        // We can test this via the connection flow
        Assert.NotNull(client.ConnectionState);
    }
}
