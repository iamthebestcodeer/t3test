using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class ServerModelsTests
{
    [Fact]
    public void ServerConfig_StoresCorrectly()
    {
        var config = new ServerConfig
        {
            Port = 9222,
            Host = "127.0.0.1",
            Mode = "desktop",
            T3Home = "/home/user/.t3",
            Version = "1.0.0",
            HasAuthToken = true,
        };

        Assert.Equal(9222, config.Port);
        Assert.Equal("127.0.0.1", config.Host);
        Assert.Equal("desktop", config.Mode);
        Assert.Equal("/home/user/.t3", config.T3Home);
        Assert.Equal("1.0.0", config.Version);
        Assert.True(config.HasAuthToken);
    }

    [Fact]
    public void ServerSettings_Defaults_Null()
    {
        var settings = new ServerSettings();

        Assert.Null(settings.DefaultRuntimeMode);
        Assert.Null(settings.DefaultInteractionMode);
        Assert.Null(settings.AutoBootstrapProjectFromCwd);
        Assert.Null(settings.LogWebSocketEvents);
    }

    [Fact]
    public void RuntimeMode_AllValues_Covered()
    {
        var values = Enum.GetValues<RuntimeMode>();
        Assert.Equal(3, values.Length);
        Assert.Contains(RuntimeMode.Web, values);
        Assert.Contains(RuntimeMode.Desktop, values);
        Assert.Contains(RuntimeMode.Terminal, values);
    }

    [Fact]
    public void InteractionMode_AllValues_Covered()
    {
        var values = Enum.GetValues<InteractionMode>();
        Assert.Equal(2, values.Length);
        Assert.Contains(InteractionMode.Default, values);
        Assert.Contains(InteractionMode.Plan, values);
    }

    [Fact]
    public void ProviderInfo_StoresCorrectly()
    {
        var provider = new ProviderInfo
        {
            Name = "codex",
            Available = true,
            Version = "2.0.1",
        };

        Assert.Equal("codex", provider.Name);
        Assert.True(provider.Available);
        Assert.Equal("2.0.1", provider.Version);
    }

    [Fact]
    public void ServerStatus_StoresCorrectly()
    {
        var status = new ServerStatus
        {
            State = "running",
            Config = new ServerConfig { Port = 9222, Mode = "desktop" },
            Providers = [new ProviderInfo { Name = "codex" }],
        };

        Assert.Equal("running", status.State);
        Assert.NotNull(status.Config);
        Assert.Single(status.Providers);
    }
}
