namespace T3Code.Core.Models;

public sealed record ServerConfig
{
    public required int Port { get; init; }
    public string? Host { get; init; }
    public required string Mode { get; init; }
    public string? T3Home { get; init; }
    public string? Version { get; init; }
    public bool HasAuthToken { get; init; }
}

public sealed record ServerSettings
{
    public string? DefaultRuntimeMode { get; init; }
    public string? DefaultInteractionMode { get; init; }
    public bool? AutoBootstrapProjectFromCwd { get; init; }
    public bool? LogWebSocketEvents { get; init; }
    public string? OtlpTracesUrl { get; init; }
    public string? OtlpMetricsUrl { get; init; }
}

public sealed record ProviderInfo
{
    public required string Name { get; init; }
    public bool Available { get; init; }
    public string? Version { get; init; }
}

public sealed record ServerStatus
{
    public required string State { get; init; }
    public ServerConfig? Config { get; init; }
    public IReadOnlyList<ProviderInfo> Providers { get; init; } = [];
    public DateTime UpdatedAt { get; init; }
}

public enum RuntimeMode
{
    Web,
    Desktop,
    Terminal,
}

public enum InteractionMode
{
    Default,
    Plan,
}
