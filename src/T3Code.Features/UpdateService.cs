using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Client;

namespace T3Code.Features;

public sealed class UpdateService : IAsyncDisposable
{
    private readonly ITransportClient _transport;
    private readonly ISettingsStore _store;
    private volatile bool _disposed;

    public event Action<DesktopUpdateState>? UpdateStateChanged;

    public DesktopUpdateState CurrentState { get; private set; } = DesktopUpdateState.Disabled("0.1.0");

    public UpdateService(ITransportClient transport, ISettingsStore store)
    {
        _transport = transport;
        _store = store;
    }

    public Task<DesktopUpdateState> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("o");
        var state = CurrentState with
        {
            Status = UpdateStatus.Checking,
            Message = null,
        };
        SetState(state);

        // In a WinUI app, update checks would go through the Windows App SDK or a custom update server.
        // For now, report up-to-date.
        var result = CurrentState with
        {
            Status = UpdateStatus.UpToDate,
        };
        SetState(result);
        return Task.FromResult(result);
    }

    public Task<DesktopUpdateActionResult> DownloadUpdateAsync(CancellationToken cancellationToken = default)
    {
        var result = new DesktopUpdateActionResult
        {
            Accepted = false,
            Completed = false,
            State = CurrentState,
        };
        return Task.FromResult(result);
    }

    public Task<DesktopUpdateActionResult> InstallUpdateAsync(CancellationToken cancellationToken = default)
    {
        var result = new DesktopUpdateActionResult
        {
            Accepted = false,
            Completed = false,
            State = CurrentState,
        };
        return Task.FromResult(result);
    }

    private void SetState(DesktopUpdateState state)
    {
        CurrentState = state;
        _store.UpdateDesktopState(state);
        UpdateStateChanged?.Invoke(state);
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        return ValueTask.CompletedTask;
    }
}
