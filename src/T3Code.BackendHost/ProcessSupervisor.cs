using System.Diagnostics;
using System.Security.Cryptography;
using T3Code.Core.Abstractions;
using T3Code.Core.Models;

namespace T3Code.BackendHost;

public sealed class ProcessSupervisor : IBackendHost, IDisposable
{

    private readonly object _lock = new();
    private readonly List<string> _logBuffer = [];

    private Process? _process;
    private BackendConfig? _config;
    private CancellationTokenSource? _cts;
    private int _restartAttempts;
    private volatile bool _disposed;
    private volatile bool _isRunning;

    public bool IsRunning => _isRunning;
    public int BackendPid
    {
        get { lock (_lock) return _process?.Id ?? 0; }
    }

    public event EventHandler<BackendExitEventArgs>? BackendExited;

    public IReadOnlyList<string> LogBuffer
    {
        get { lock (_lock) return _logBuffer.ToList().AsReadOnly(); }
    }

    public async Task<ConnectionInfo> StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_config == null)
        {
            throw new InvalidOperationException("Configuration not set. Call Configure() first.");
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var info = await StartInternalAsync(_cts.Token);
        return info;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning) return;

        _cts?.Cancel();

        Process? proc;
        lock (_lock)
        {
            proc = _process;
            _process = null;
            _isRunning = false;
        }

        if (proc != null)
        {
            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill(entireProcessTree: true);

                    using var exitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    exitCts.CancelAfter(_config?.ProcessExitTimeout ?? TimeSpan.FromSeconds(5));

                    try
                    {
                        await proc.WaitForExitAsync(exitCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Force kill if graceful shutdown fails
                        try { proc.Kill(entireProcessTree: true); }
                        catch { /* already dead */ }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Process already exited
            }

            proc.Dispose();
        }
    }

    public async Task<ConnectionInfo> RestartAsync(CancellationToken cancellationToken = default)
    {
        await StopAsync(cancellationToken);
        await Task.Delay(500, cancellationToken);
        return await StartAsync(cancellationToken);
    }

    public void Configure(BackendConfig config)
    {
        _config = config;
    }

    public static string GenerateAuthToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task<ConnectionInfo> StartInternalAsync(CancellationToken cancellationToken)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Configuration not set");
        }

        var port = PortAllocator.ReservePort();
        var authToken = GenerateAuthToken();

        var arguments = $"--mode {_config.Mode} --port {port} --auth-token {authToken} --no-browser";
        if (!string.IsNullOrEmpty(_config.T3Home))
        {
            arguments += $" --base-dir \"{_config.T3Home}\"";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _config.ExecutablePath,
            Arguments = arguments,
            WorkingDirectory = _config.Cwd,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        // Set environment variables
        if (_config.ExtraEnvironment != null)
        {
            foreach (var kvp in _config.ExtraEnvironment)
            {
                startInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start backend process");
        }

        lock (_lock)
        {
            _process = process;
            _isRunning = true;
            _restartAttempts = 0;
        }

        _ = Task.Run(() => MonitorProcessAsync(process), cancellationToken);
        _ = Task.Run(() => CaptureOutput(process), cancellationToken);

        // Wait a moment for the server to start listening
        await Task.Delay(1000, cancellationToken);

        return new ConnectionInfo
        {
            WebSocketUrl = $"ws://127.0.0.1:{port}/?token={authToken}",
            Port = port,
            AuthToken = authToken,
            T3Home = _config.T3Home,
        };
    }

    private async Task MonitorProcessAsync(Process process)
    {
        try
        {
            await process.WaitForExitAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        lock (_lock)
        {
            _isRunning = false;
        }

        BackendExited?.Invoke(this, new BackendExitEventArgs
        {
            ExitCode = process.ExitCode,
            ExitTime = DateTime.UtcNow,
        });

        // Auto-restart logic
        if (_cts?.IsCancellationRequested != true &&
            _config != null &&
            _restartAttempts < _config.MaxRestartAttempts)
        {
            _restartAttempts++;
            var delay = _config.RestartDelayBase * _restartAttempts;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, _cts?.Token ?? CancellationToken.None);
                    if (_cts?.IsCancellationRequested != true)
                    {
                        await StartInternalAsync(_cts?.Token ?? CancellationToken.None);
                    }
                }
                catch (OperationCanceledException) { }
            });
        }
    }

    private void CaptureOutput(Process process)
    {
        try
        {
            _ = Task.Run(async () =>
            {
                using var reader = process.StandardOutput;
                while (await reader.ReadLineAsync() is { } line)
                {
                    lock (_lock)
                    {
                        _logBuffer.Add(line);
                        if (_logBuffer.Count > 1000)
                        {
                            _logBuffer.RemoveAt(0);
                        }
                    }
                }
            });

            _ = Task.Run(async () =>
            {
                using var reader = process.StandardError;
                while (await reader.ReadLineAsync() is { } line)
                {
                    lock (_lock)
                    {
                        _logBuffer.Add($"[stderr] {line}");
                        if (_logBuffer.Count > 1000)
                        {
                            _logBuffer.RemoveAt(0);
                        }
                    }
                }
            });
        }
        catch
        {
            // Swallow stream errors during process shutdown
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();

        try
        {
            _process?.Kill(entireProcessTree: true);
            _process?.Dispose();
        }
        catch
        {
            // Swallow dispose errors
        }
    }
}
