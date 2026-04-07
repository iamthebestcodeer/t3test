using System.Collections.ObjectModel;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using T3Code.Core.Models;

namespace T3Code.App.Views
{
    public sealed record TerminalSessionViewModel
    {
        public required string SessionId { get; init; }
        public required string Title { get; init; }
        public required string Cwd { get; init; }
        public int Cols { get; init; }
        public int Rows { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed class ProjectItemViewModel
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Cwd { get; init; }
    }

    public partial class TerminalPage : Page
    {
        private readonly ObservableCollection<TerminalSessionViewModel> _sessions = [];
        private readonly ObservableCollection<ProjectItemViewModel> _projects = [];
        private readonly Dictionary<string, StringBuilder> _outputBuffers = new();
        private string? _selectedProjectId;
        private string? _selectedProjectCwd;
        private Action<OrchestrationSnapshot?>? _snapshotChangedHandler;
        private Action<IReadOnlyList<TerminalSession>>? _sessionsChangedHandler;
        private Action<TerminalEvent>? _terminalEventHandler;

        public TerminalPage()
        {
            this.InitializeComponent();
            SubscribeToEvents();
            LoadProjects();
            this.Unloaded += (_, _) => UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            var services = App.Services;

            _snapshotChangedHandler = _ => DispatcherQueue.TryEnqueue(LoadProjects);
            services.OrchestrationStore.SnapshotChanged += _snapshotChangedHandler;

            _sessionsChangedHandler = sessions => DispatcherQueue.TryEnqueue(() => RefreshSessionsList(sessions));
            services.TerminalStore.SessionsChanged += _sessionsChangedHandler;

            _terminalEventHandler = evt => DispatcherQueue.TryEnqueue(() =>
            {
                if (evt.Type == TerminalEventType.Data && evt.Data != null)
                {
                    AppendOutput(evt.SessionId.Value, evt.Data);
                }
                else if (evt.Type == TerminalEventType.Title && evt.Data != null)
                {
                    UpdateSessionTitle(evt.SessionId.Value, evt.Data);
                }
                else if (evt.Type == TerminalEventType.Closed || evt.Type == TerminalEventType.Exit)
                {
                    AppendOutput(evt.SessionId.Value, $"\n[Session closed at {DateTime.Now:HH:mm:ss}]\n");
                }
            });
            services.TerminalStore.TerminalEventReceived += _terminalEventHandler;
        }

        private void UnsubscribeFromEvents()
        {
            var services = App.Services;

            if (_snapshotChangedHandler != null)
                services.OrchestrationStore.SnapshotChanged -= _snapshotChangedHandler;
            if (_sessionsChangedHandler != null)
                services.TerminalStore.SessionsChanged -= _sessionsChangedHandler;
            if (_terminalEventHandler != null)
                services.TerminalStore.TerminalEventReceived -= _terminalEventHandler;
        }

        private void LoadProjects()
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null) return;

            _projects.Clear();
            foreach (var p in snapshot.Projects)
            {
                _projects.Add(new ProjectItemViewModel
                {
                    Id = p.Id.Value,
                    Title = p.Title,
                    Cwd = p.Cwd,
                });
            }

            ProjectSelector.ItemsSource = null;
            ProjectSelector.ItemsSource = _projects;
            ProjectSelector.DisplayMemberPath = "Title";
        }

        private void OnProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectSelector.SelectedItem is not ProjectItemViewModel selected)
                return;

            _selectedProjectId = selected.Id;
            _selectedProjectCwd = selected.Cwd;
        }

        private void OnSessionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (SessionSelector.SelectedItem is not TerminalSessionViewModel selected)
                return;

            App.Services.TerminalStore.SetActive(TerminalSessionId.From(selected.SessionId));
            TerminalOutput.Text = _outputBuffers.TryGetValue(selected.SessionId, out var buffer) ? buffer.ToString() : string.Empty;
        }

        private async void OnNewSession(object sender, RoutedEventArgs e)
        {
            var cwd = _selectedProjectCwd;
            if (string.IsNullOrEmpty(cwd))
            {
                TerminalOutput.Text = "Select a project first to open a terminal session.";
                return;
            }

            try
            {
                await App.Services.TerminalService.OpenAsync(cwd, 80, 24);
            }
            catch (Exception ex)
            {
                TerminalOutput.Text = $"Failed to open terminal: {ex.Message}";
            }
        }

        private async void OnCloseSession(object sender, RoutedEventArgs e)
        {
            if (SessionSelector.SelectedItem is not TerminalSessionViewModel selected)
                return;

            try
            {
                await App.Services.TerminalService.CloseAsync(TerminalSessionId.From(selected.SessionId));
            }
            catch (Exception ex)
            {
                TerminalOutput.Text = $"Failed to close terminal: {ex.Message}";
            }
        }

        private async void OnRestartSession(object sender, RoutedEventArgs e)
        {
            if (SessionSelector.SelectedItem is not TerminalSessionViewModel selected)
                return;

            try
            {
                await App.Services.TerminalService.RestartAsync(TerminalSessionId.From(selected.SessionId));
            }
            catch (Exception ex)
            {
                TerminalOutput.Text = $"Failed to restart terminal: {ex.Message}";
            }
        }

        private async void OnTerminalInputKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (SessionSelector.SelectedItem is not TerminalSessionViewModel selected)
                    return;

                var text = TerminalInput.Text;
                TerminalInput.Text = string.Empty;

                if (!string.IsNullOrEmpty(text))
                {
                    AppendOutput(selected.SessionId, $"{text}\n");
                    try
                    {
                        await App.Services.TerminalService.WriteAsync(TerminalSessionId.From(selected.SessionId), text + "\n");
                    }
                    catch (Exception ex)
                    {
                        AppendOutput(selected.SessionId, $"[Error: {ex.Message}]\n");
                    }
                }
            }
        }

        private void RefreshSessionsList(IReadOnlyList<TerminalSession> sessions)
        {
            _sessions.Clear();
            foreach (var s in sessions)
            {
                _sessions.Add(new TerminalSessionViewModel
                {
                    SessionId = s.Id.Value,
                    Title = s.Title ?? $"Session {s.Id.Value[..8]}",
                    Cwd = s.Cwd ?? "",
                    Cols = s.Cols,
                    Rows = s.Rows,
                    CreatedAt = s.CreatedAt,
                });
            }

            SessionSelector.ItemsSource = null;
            SessionSelector.ItemsSource = _sessions;
            SessionSelector.DisplayMemberPath = "Title";

            var active = App.Services.TerminalStore.ActiveSession;
            if (active != null)
            {
                if (!_outputBuffers.ContainsKey(active.Id.Value))
                {
                    _outputBuffers[active.Id.Value] = new StringBuilder();
                }
                var activeVm = _sessions.FirstOrDefault(s => s.SessionId == active.Id.Value);
                if (activeVm != null)
                {
                    SessionSelector.SelectedItem = activeVm;
                }
                TerminalOutput.Text = _outputBuffers.TryGetValue(active.Id.Value, out var buffer) ? buffer.ToString() : string.Empty;
            }
        }

        private void AppendOutput(string sessionId, string data)
        {
            if (!_outputBuffers.TryGetValue(sessionId, out var buffer))
            {
                buffer = new StringBuilder();
                _outputBuffers[sessionId] = buffer;
            }

            buffer.Append(data);

            // Trim buffer if too large - keep most recent 40000 chars
            if (buffer.Length > 50000)
            {
                buffer.Remove(0, buffer.Length - 40000);
            }

            var active = App.Services.TerminalStore.ActiveSession;
            if (active?.Id.Value == sessionId)
            {
                TerminalOutput.Text = buffer.ToString();
            }
        }

        private void UpdateSessionTitle(string sessionId, string title)
        {
            for (int i = 0; i < _sessions.Count; i++)
            {
                if (_sessions[i].SessionId == sessionId)
                {
                    _sessions[i] = _sessions[i] with { Title = title };
                    break;
                }
            }
        }
    }
}
