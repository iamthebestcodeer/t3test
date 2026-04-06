using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class DesktopModelsTests
{
    [Fact]
    public void Theme_AllValues_Covered()
    {
        var values = Enum.GetValues<Theme>();
        Assert.Equal(3, values.Length);
        Assert.Contains(Theme.Light, values);
        Assert.Contains(Theme.Dark, values);
        Assert.Contains(Theme.System, values);
    }

    [Fact]
    public void Keybinding_StoresCorrectly()
    {
        var binding = new Keybinding
        {
            Key = "Ctrl+N",
            Command = "chat.new",
            When = "!terminalFocus",
        };

        Assert.Equal("Ctrl+N", binding.Key);
        Assert.Equal("chat.new", binding.Command);
        Assert.Equal("!terminalFocus", binding.When);
        Assert.True(binding.IsDefault);
    }

    [Fact]
    public void KeybindingsConfig_Empty_CreatesCorrectly()
    {
        var config = KeybindingsConfig.Empty;

        Assert.Empty(config.Rules);
        Assert.True(config.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateStatus_AllValues_Covered()
    {
        var values = Enum.GetValues<UpdateStatus>();
        Assert.Equal(8, values.Length);
        Assert.Contains(UpdateStatus.Disabled, values);
        Assert.Contains(UpdateStatus.Idle, values);
        Assert.Contains(UpdateStatus.Checking, values);
        Assert.Contains(UpdateStatus.UpToDate, values);
        Assert.Contains(UpdateStatus.Available, values);
        Assert.Contains(UpdateStatus.Downloading, values);
        Assert.Contains(UpdateStatus.Downloaded, values);
        Assert.Contains(UpdateStatus.Error, values);
    }

    [Fact]
    public void DesktopUpdateState_Disabled_CreatesCorrectly()
    {
        var state = DesktopUpdateState.Disabled("1.0.0");

        Assert.False(state.Enabled);
        Assert.Equal(UpdateStatus.Disabled, state.Status);
        Assert.Equal("1.0.0", state.CurrentVersion);
    }

    [Fact]
    public void DesktopUpdateState_StoresAllFields()
    {
        var state = new DesktopUpdateState
        {
            Enabled = true,
            Status = UpdateStatus.Downloading,
            CurrentVersion = "1.0.0",
            AvailableVersion = "1.1.0",
            DownloadPercent = 45.5,
            Message = "Downloading update...",
            CanRetry = false,
        };

        Assert.True(state.Enabled);
        Assert.Equal(UpdateStatus.Downloading, state.Status);
        Assert.Equal("1.1.0", state.AvailableVersion);
        Assert.Equal(45.5, state.DownloadPercent);
        Assert.Equal("Downloading update...", state.Message);
    }

    [Fact]
    public void DesktopUpdateCheckResult_StoresCorrectly()
    {
        var result = new DesktopUpdateCheckResult
        {
            Checked = true,
            State = DesktopUpdateState.Disabled("1.0.0"),
        };

        Assert.True(result.Checked);
        Assert.NotNull(result.State);
    }

    [Fact]
    public void DesktopUpdateActionResult_StoresCorrectly()
    {
        var result = new DesktopUpdateActionResult
        {
            Accepted = true,
            Completed = false,
            State = DesktopUpdateState.Disabled("1.0.0"),
        };

        Assert.True(result.Accepted);
        Assert.False(result.Completed);
    }
}
