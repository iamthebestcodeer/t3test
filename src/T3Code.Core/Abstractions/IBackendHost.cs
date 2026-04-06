using T3Code.Core.Models;

namespace T3Code.Core.Abstractions;

public interface IBackendHost
{
    Task<ConnectionInfo> StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<ConnectionInfo> RestartAsync(CancellationToken cancellationToken = default);
    bool IsRunning { get; }
    int BackendPid { get; }
    event EventHandler<BackendExitEventArgs>? BackendExited;
}

public sealed class BackendExitEventArgs : EventArgs
{
    public required int ExitCode { get; init; }
    public string? Signal { get; init; }
    public DateTime ExitTime { get; init; }
}
