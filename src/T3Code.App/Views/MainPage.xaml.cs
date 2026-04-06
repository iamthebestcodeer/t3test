namespace T3Code.App.Views
{
    public sealed class ProjectViewModel
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Cwd { get; init; }
    }

    public partial class MainPage : Page
    {
        private readonly List<ProjectViewModel> _projects = [];

        public MainPage()
        {
            this.InitializeComponent();
            UpdateConnectionStatus();
            SubscribeToEvents();

            this.Loaded += (_, _) => LoadDataAsync();
        }

        private void SubscribeToEvents()
        {
            var services = App.Services;

            services.ConnectionStore.StateChanged += state =>
            {
                DispatcherQueue.TryEnqueue(() => UpdateConnectionStatus());
            };

            services.OrchestrationStore.SnapshotChanged += snapshot =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _projects.Clear();
                    foreach (var p in snapshot.Projects)
                    {
                        _projects.Add(new ProjectViewModel
                        {
                            Id = p.Id.Value,
                            Title = p.Title,
                            Cwd = p.Cwd,
                        });
                    }
                    ProjectsList.ItemsSource = null;
                    ProjectsList.ItemsSource = _projects;
                });
            };
        }

        private void UpdateConnectionStatus()
        {
            var state = App.Services.ConnectionStore.State;
            ConnectionStatusText.Text = state.State switch
            {
                Core.Models.ConnectionState.Connected => "Connected",
                Core.Models.ConnectionState.Connecting => "Connecting...",
                Core.Models.ConnectionState.Reconnecting => $"Reconnecting (attempt {state.ReconnectAttempt})...",
                Core.Models.ConnectionState.Disconnected => "Disconnected",
                Core.Models.ConnectionState.Failed => $"Failed: {state.LastError}",
                _ => "Unknown",
            };

            var pid = App.Services.BackendHost.BackendPid;
            BackendPidText.Text = pid > 0 ? $"Backend PID: {pid}" : string.Empty;
        }

        private async void LoadDataAsync()
        {
            try
            {
                if (App.Services.ConnectionStore.State.State == Core.Models.ConnectionState.Connected)
                {
                    await App.Services.OrchestrationService.LoadSnapshotAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load snapshot failed: {ex.Message}");
            }
        }
    }
}
