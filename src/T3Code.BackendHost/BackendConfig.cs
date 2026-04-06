namespace T3Code.BackendHost;

public sealed record BackendConfig
{
    public required string ExecutablePath { get; init; }
    public required string Cwd { get; init; }
    public required int Port { get; init; }
    public required string AuthToken { get; init; }
    public string? T3Home { get; init; }
    public string Mode { get; init; } = "desktop";
    public bool NoBrowser { get; init; } = true;
    public Dictionary<string, string>? ExtraEnvironment { get; init; }
    public TimeSpan RestartDelayBase { get; init; } = TimeSpan.FromMilliseconds(500);
    public int MaxRestartAttempts { get; init; } = 10;
    public TimeSpan ProcessExitTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
