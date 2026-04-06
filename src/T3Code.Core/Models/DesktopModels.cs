namespace T3Code.Core.Models;

public enum Theme
{
    Light,
    Dark,
    System,
}

public sealed record Keybinding
{
    public required string Key { get; init; }
    public required string Command { get; init; }
    public string? When { get; init; }
    public bool IsDefault { get; init; } = true;
}

public sealed record KeybindingsConfig
{
    public required IReadOnlyList<Keybinding> Rules { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static KeybindingsConfig Empty => new()
    {
        Rules = [],
        UpdatedAt = DateTime.UtcNow,
    };
}

public sealed record DesktopUpdateState
{
    public required bool Enabled { get; init; }
    public required UpdateStatus Status { get; init; }
    public required string CurrentVersion { get; init; }
    public string? AvailableVersion { get; init; }
    public string? DownloadedVersion { get; init; }
    public double? DownloadPercent { get; init; }
    public string? Message { get; init; }
    public bool CanRetry { get; init; }

    public static DesktopUpdateState Disabled(string version) => new()
    {
        Enabled = false,
        Status = UpdateStatus.Disabled,
        CurrentVersion = version,
    };
}

public enum UpdateStatus
{
    Disabled,
    Idle,
    Checking,
    UpToDate,
    Available,
    Downloading,
    Downloaded,
    Error,
}

public sealed record DesktopUpdateCheckResult
{
    public required bool Checked { get; init; }
    public required DesktopUpdateState State { get; init; }
}

public sealed record DesktopUpdateActionResult
{
    public required bool Accepted { get; init; }
    public required bool Completed { get; init; }
    public required DesktopUpdateState State { get; init; }
}
