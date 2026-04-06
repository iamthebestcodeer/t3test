using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Client;

namespace T3Code.Features;

public sealed class SettingsService : IAsyncDisposable
{
    private readonly ITransportClient _transport;
    private readonly ISettingsStore _store;
    private volatile bool _disposed;

    public SettingsService(ITransportClient transport, ISettingsStore store)
    {
        _transport = transport;
        _store = store;
    }

    public async Task<ServerConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        return await _transport.RequestAsync<ServerConfig>(
            "server.getConfig",
            null,
            cancellationToken);
    }

    public async Task<ServerSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _transport.RequestAsync<ServerSettings>(
            "server.getSettings",
            null,
            cancellationToken);

        _store.UpdateSettings(settings);
        return settings;
    }

    public async Task<ServerSettings> UpdateSettingsAsync(
        Dictionary<string, object> patch,
        CancellationToken cancellationToken = default)
    {
        var settings = await _transport.RequestAsync<ServerSettings>(
            "server.updateSettings",
            patch,
            cancellationToken);

        _store.UpdateSettings(settings);
        return settings;
    }

    public async Task<IReadOnlyList<ProviderInfo>> RefreshProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _transport.RequestListAsync<ProviderInfo>(
            "server.refreshProviders",
            null,
            cancellationToken);
    }

    public void UpdateDesktopState(DesktopUpdateState state)
    {
        _store.UpdateDesktopState(state);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await Task.CompletedTask;
    }
}
