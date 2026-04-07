using T3Code.Core.Abstractions;
using T3Code.Core.State;
using T3Code.BackendHost;
using T3Code.Platform;
using T3Code.Features;
using T3Code.Transport.Client;

namespace T3Code.App;

public sealed class AppServices : IAsyncDisposable
{
    public ConnectionStore ConnectionStore { get; }
    public OrchestrationStore OrchestrationStore { get; }
    public TerminalStore TerminalStore { get; }
    public GitStore GitStore { get; }
    public SettingsStore SettingsStore { get; }

    public ProcessSupervisor BackendHost { get; }
    public WebSocketTransportClient TransportClient { get; }
    public ExternalLinkService ExternalLinkService { get; }
    public ThemeService ThemeService { get; }
    public AppStatePaths Paths { get; }

    public OrchestrationService OrchestrationService { get; }
    public TerminalService TerminalService { get; }
    public GitService GitService { get; }
    public SettingsService SettingsService { get; }
    public UpdateService UpdateService { get; }

    public AppServices()
    {
        // Stores
        ConnectionStore = new ConnectionStore();
        OrchestrationStore = new OrchestrationStore();
        TerminalStore = new TerminalStore();
        GitStore = new GitStore();
        SettingsStore = new SettingsStore();

        // Platform services
        ExternalLinkService = new ExternalLinkService();
        ThemeService = new ThemeService();
        Paths = new AppStatePaths();
        Paths.EnsureDirectories();

        // Transport & host
        BackendHost = new ProcessSupervisor();
        TransportClient = new WebSocketTransportClient();

        // Feature services
        OrchestrationService = new OrchestrationService(TransportClient, OrchestrationStore);
        TerminalService = new TerminalService(TransportClient, TerminalStore);
        GitService = new GitService(TransportClient, GitStore);
        SettingsService = new SettingsService(TransportClient, SettingsStore);
        UpdateService = new UpdateService(TransportClient, SettingsStore);

        // Wire up connection state
        TransportClient.ConnectionStateChanged += state =>
        {
            ConnectionStore.UpdateState(state);
        };
    }

    public async ValueTask DisposeAsync()
    {
        await UpdateService.DisposeAsync();
        await OrchestrationService.DisposeAsync();
        await TerminalService.DisposeAsync();
        await GitService.DisposeAsync();
        await SettingsService.DisposeAsync();
        await TransportClient.DisposeAsync();
        BackendHost.Dispose();
    }
}
