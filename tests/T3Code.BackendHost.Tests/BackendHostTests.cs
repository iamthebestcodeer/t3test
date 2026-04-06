using T3Code.BackendHost;

namespace T3Code.BackendHost.Tests;

public class ProcessSupervisorTests
{
    [Fact]
    public void InitialState_NotRunning()
    {
        using var supervisor = new ProcessSupervisor();

        Assert.False(supervisor.IsRunning);
        Assert.Equal(0, supervisor.BackendPid);
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_DoesNotThrow()
    {
        using var supervisor = new ProcessSupervisor();

        // Should not throw
        await supervisor.StopAsync();
    }

    [Fact]
    public void Configure_StoresConfig()
    {
        using var supervisor = new ProcessSupervisor();
        var config = new BackendConfig
        {
            ExecutablePath = "node",
            Cwd = "/tmp",
            Port = 9222,
            AuthToken = "test-token",
        };

        supervisor.Configure(config);

        // Config is stored (no direct getter, but StartAsync would use it)
    }

    [Fact]
    public async Task StartAsync_WithoutConfigure_Throws()
    {
        using var supervisor = new ProcessSupervisor();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => supervisor.StartAsync());
    }

    [Fact]
    public void GenerateAuthToken_ReturnsNonEmptyString()
    {
        var token = ProcessSupervisor.GenerateAuthToken();

        Assert.False(string.IsNullOrEmpty(token));
        Assert.True(token.Length >= 32); // 32 bytes = 64 hex chars
    }

    [Fact]
    public void GenerateAuthToken_ReturnsUniqueValues()
    {
        var token1 = ProcessSupervisor.GenerateAuthToken();
        var token2 = ProcessSupervisor.GenerateAuthToken();

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateAuthToken_IsHexString()
    {
        var token = ProcessSupervisor.GenerateAuthToken();

        Assert.Matches(@"^[0-9a-f]+$", token);
    }

    [Fact]
    public async Task Dispose_DoesNotThrow()
    {
        var supervisor = new ProcessSupervisor();

        // Should not throw even if not started
        supervisor.Dispose();

        // Multiple dispose calls should be safe
        supervisor.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public void LogBuffer_InitializesEmpty()
    {
        using var supervisor = new ProcessSupervisor();

        Assert.Empty(supervisor.LogBuffer);
    }
}

public class PortAllocatorTests
{
    [Fact]
    public void ReservePort_ReturnsPositivePort()
    {
        var port = PortAllocator.ReservePort();

        Assert.True(port > 0);
        Assert.True(port <= 65535);
    }

    [Fact]
    public void ReservePort_ReturnsDifferentPorts()
    {
        var port1 = PortAllocator.ReservePort();
        var port2 = PortAllocator.ReservePort();

        Assert.NotEqual(port1, port2);
    }

    [Fact]
    public void GetRandomEphemeralPort_InValidRange()
    {
        var allocator = new PortAllocator();
        var port = allocator.GetRandomEphemeralPort();

        Assert.True(port >= 49152);
        Assert.True(port <= 65535);
    }

    [Fact]
    public void ReservePort_WithSpecificHost()
    {
        var port = PortAllocator.ReservePort("127.0.0.1");

        Assert.True(port > 0);
    }
}

public class BackendConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new BackendConfig
        {
            ExecutablePath = "node",
            Cwd = "/tmp",
            Port = 9222,
            AuthToken = "token",
        };

        Assert.Equal("desktop", config.Mode);
        Assert.True(config.NoBrowser);
        Assert.Equal(TimeSpan.FromMilliseconds(500), config.RestartDelayBase);
        Assert.Equal(10, config.MaxRestartAttempts);
        Assert.Equal(TimeSpan.FromSeconds(5), config.ProcessExitTimeout);
    }

    [Fact]
    public void Config_StoresAllFields()
    {
        var config = new BackendConfig
        {
            ExecutablePath = "node",
            Cwd = "/app",
            Port = 8080,
            AuthToken = "secret",
            T3Home = "/home/.t3",
            Mode = "web",
            NoBrowser = false,
            ExtraEnvironment = new Dictionary<string, string> { ["NODE_ENV"] = "production" },
            RestartDelayBase = TimeSpan.FromSeconds(1),
            MaxRestartAttempts = 5,
        };

        Assert.Equal("node", config.ExecutablePath);
        Assert.Equal("/app", config.Cwd);
        Assert.Equal(8080, config.Port);
        Assert.Equal("secret", config.AuthToken);
        Assert.Equal("/home/.t3", config.T3Home);
        Assert.Equal("web", config.Mode);
        Assert.False(config.NoBrowser);
        Assert.Single(config.ExtraEnvironment!);
        Assert.Equal(TimeSpan.FromSeconds(1), config.RestartDelayBase);
        Assert.Equal(5, config.MaxRestartAttempts);
    }
}
