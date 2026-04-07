using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using T3Code.Core.Models;

namespace T3Code.App.Views
{
    public sealed class TreeItemViewModel
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Subtitle { get; init; }
        public required string Glyph { get; init; }
        public bool IsProject { get; init; }
    }

    public sealed class MessageViewModel
    {
        public required string Id { get; init; }
        public required string Text { get; init; }
        public required string RoleLabel { get; init; }
        public required string RoleGlyph { get; init; }
        public required string Timestamp { get; init; }
        public required string MessageBackground { get; init; }
    }

    public partial class MainPage : Page
    {
        private readonly List<TreeItemViewModel> _treeItems = [];
        private readonly List<MessageViewModel> _messages = [];
        private string? _selectedThreadId;
        private string? _selectedProjectId;
        private Action<ConnectionStateSnapshot>? _stateChangedHandler;
        private Action<OrchestrationSnapshot?>? _snapshotChangedHandler;
        private Action<OrchestrationEvent>? _eventReceivedHandler;
        private Action<ConnectionStateSnapshot>? _connectionRetryHandler;

        public MainPage()
        {
            this.InitializeComponent();
            UpdateConnectionStatus();
            SubscribeToEvents();
            this.Loaded += (_, _) => LoadDataAsync();
            this.Unloaded += (_, _) => UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            var services = App.Services;

            _stateChangedHandler = _ => DispatcherQueue.TryEnqueue(UpdateConnectionStatus);
            services.ConnectionStore.StateChanged += _stateChangedHandler;

            _connectionRetryHandler = state =>
            {
                if (state.State == ConnectionState.Connected)
                {
                    DispatcherQueue.TryEnqueue(() => LoadDataAsync());
                }
            };
            services.ConnectionStore.StateChanged += _connectionRetryHandler;

            _snapshotChangedHandler = snapshot => DispatcherQueue.TryEnqueue(() =>
            {
                if (snapshot != null) BuildProjectTree(snapshot);
            });
            services.OrchestrationStore.SnapshotChanged += _snapshotChangedHandler;

            _eventReceivedHandler = evt => DispatcherQueue.TryEnqueue(() => HandleOrchestrationEvent(evt));
            services.OrchestrationStore.EventReceived += _eventReceivedHandler;
        }

        private void UnsubscribeFromEvents()
        {
            var services = App.Services;

            if (_stateChangedHandler != null)
                services.ConnectionStore.StateChanged -= _stateChangedHandler;
            if (_connectionRetryHandler != null)
                services.ConnectionStore.StateChanged -= _connectionRetryHandler;
            if (_snapshotChangedHandler != null)
                services.OrchestrationStore.SnapshotChanged -= _snapshotChangedHandler;
            if (_eventReceivedHandler != null)
                services.OrchestrationStore.EventReceived -= _eventReceivedHandler;
        }

        private void UpdateConnectionStatus()
        {
            var state = App.Services.ConnectionStore.State;
            var statusText = state.State switch
            {
                ConnectionState.Connected => "Connected",
                ConnectionState.Connecting => "Connecting...",
                ConnectionState.Reconnecting => $"Reconnecting (attempt {state.ReconnectAttempt})...",
                ConnectionState.Disconnected => "Disconnected",
                ConnectionState.Failed => $"Failed: {state.LastError}",
                _ => "Unknown",
            };

            ConnectionStatusText.Text = statusText;

            var pid = App.Services.BackendHost.BackendPid;
            BackendPidText.Text = pid > 0 ? $"PID {pid}" : string.Empty;

            // Update icon
            StatusIcon.Glyph = state.State switch
            {
                ConnectionState.Connected => "\uE930", // CheckMark
                ConnectionState.Failed => "\uE783",    // Error
                _ => "\uE894",                          // Sync
            };

            NewThreadBtn.IsEnabled = state.State == ConnectionState.Connected;
            SendBtn.IsEnabled = state.State == ConnectionState.Connected && _selectedThreadId != null;
        }

        private void BuildProjectTree(OrchestrationSnapshot snapshot)
        {
            _treeItems.Clear();

            foreach (var project in snapshot.Projects)
            {
                var projectThreads = snapshot.Threads
                    .Where(t => t.ProjectId.Value == project.Id.Value)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToList();

                _treeItems.Add(new TreeItemViewModel
                {
                    Id = project.Id.Value,
                    Title = project.Title,
                    Subtitle = project.Cwd,
                    Glyph = "\uE8B7",
                    IsProject = true,
                });

                foreach (var thread in projectThreads)
                {
                    _treeItems.Add(new TreeItemViewModel
                    {
                        Id = thread.Id.Value,
                        Title = thread.Title,
                        Subtitle = StatusToDisplay(thread.Status),
                        Glyph = "\uE8BD",
                        IsProject = false,
                    });
                }
            }

            ThreadList.ItemsSource = _treeItems;
        }

        private void OnThreadSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ThreadList.SelectedItem is not TreeItemViewModel item)
            {
                // Clear selections when nothing selected
                _selectedThreadId = null;
                _selectedProjectId = null;
                ThreadTitleText.Text = "Select a thread";
                SendBtn.IsEnabled = false;
                InterruptBtn.IsEnabled = false;
                DiffBtn.IsEnabled = false;
                _messages.Clear();
                MessageList.ItemsSource = null;
                return;
            }

            if (item.IsProject)
            {
                // Track selected project, clear thread selection
                _selectedProjectId = item.Id;
                _selectedThreadId = null;
                ThreadTitleText.Text = "Select a thread";
                SendBtn.IsEnabled = false;
                InterruptBtn.IsEnabled = false;
                DiffBtn.IsEnabled = false;
                _messages.Clear();
                MessageList.ItemsSource = null;
                return;
            }

            // Thread selected - track both thread and its parent project
            _selectedThreadId = item.Id;
            _selectedProjectId = GetProjectIdForThread(item.Id);
            ThreadTitleText.Text = item.Title;
            SendBtn.IsEnabled = true;
            InterruptBtn.IsEnabled = true;
            DiffBtn.IsEnabled = true;

            // Load messages for this thread from the snapshot
            LoadThreadMessages(item.Id);
        }

        private static string? GetProjectIdForThread(string threadId)
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null) return null;

            var thread = snapshot.Threads.FirstOrDefault(t => t.Id.Value == threadId);
            return thread?.ProjectId.Value;
        }

        private async void LoadThreadMessages(string threadId)
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null) return;

            var thread = snapshot.Threads.FirstOrDefault(t => t.Id.Value == threadId);
            if (thread == null) return;

            _messages.Clear();

            // Add thread info header
            _messages.Add(new MessageViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Text = $"Thread: {thread.Title}\nStatus: {StatusToDisplay(thread.Status)}\nBranch: {thread.Branch ?? "(none)"}",
                RoleLabel = "System",
                RoleGlyph = "\uE7C3",
                Timestamp = thread.CreatedAt ?? DateTime.UtcNow.ToString("o"),
                MessageBackground = "Transparent",
            });

            // Add actual messages from thread history
            foreach (var message in thread.Messages)
            {
                var (roleLabel, roleGlyph, background) = message.Role.ToLowerInvariant() switch
                {
                    "user" => ("You", "\uE77B", "#E8F4FD"),
                    "assistant" => ("Assistant", "\uE8C4", "#F0F0F0"),
                    "system" => ("System", "\uE7C3", "Transparent"),
                    _ => ("Unknown", "\uE9CE", "Transparent"),
                };

                _messages.Add(new MessageViewModel
                {
                    Id = message.Id,
                    Text = message.Text,
                    RoleLabel = roleLabel,
                    RoleGlyph = roleGlyph,
                    Timestamp = message.CreatedAt ?? thread.UpdatedAt ?? DateTime.UtcNow.ToString("o"),
                    MessageBackground = background,
                });
            }

            // Add current turn diff if available
            if (thread.CurrentTurnId != null)
            {
                try
                {
                    var diff = await App.Services.OrchestrationService.GetTurnDiffAsync(thread.CurrentTurnId.Value);
                    if (diff.Files.Count > 0)
                    {
                        var sb = new System.Text.StringBuilder();
                        foreach (var file in diff.Files)
                        {
                            sb.AppendLine($"{file.Status.ToUpperInvariant()}: {file.Path}");
                        }
                        _messages.Add(new MessageViewModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = sb.ToString(),
                            RoleLabel = "Changes",
                            RoleGlyph = "\uE8A5",
                            Timestamp = thread.UpdatedAt ?? DateTime.UtcNow.ToString("o"),
                            MessageBackground = "Transparent",
                        });
                    }
                }
                catch
                {
                    // Ignore diff fetch errors
                }
            }

            MessageList.ItemsSource = null;
            MessageList.ItemsSource = _messages;
        }

        private async void HandleOrchestrationEvent(OrchestrationEvent evt)
        {
            if (evt.Type.StartsWith("thread.") || evt.Type.StartsWith("project."))
            {
                try
                {
                    await App.Services.OrchestrationService.LoadSnapshotAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Refresh snapshot failed: {ex.Message}");
                }
            }

            if (_selectedThreadId != null)
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(evt.Payload);
                    if (doc.RootElement.TryGetProperty("threadId", out var tid))
                    {
                        if (tid.GetString() == _selectedThreadId)
                        {
                            LoadThreadMessages(_selectedThreadId);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private static async void LoadDataAsync()
        {
            try
            {
                if (App.Services.ConnectionStore.State.State == ConnectionState.Connected)
                {
                    await App.Services.OrchestrationService.LoadSnapshotAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load snapshot failed: {ex.Message}");
            }
        }

        private async void OnSendPrompt(object sender, RoutedEventArgs e)
        {
            if (_selectedThreadId == null) return;

            var prompt = PromptInput.Text?.Trim();
            if (string.IsNullOrEmpty(prompt)) return;

            PromptInput.Text = string.Empty;
            SendBtn.IsEnabled = false;

            try
            {
                await App.Services.OrchestrationService.DispatchTurnStartAsync(
                    _selectedThreadId, prompt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send prompt failed: {ex.Message}");
            }
            finally
            {
                var state = App.Services.ConnectionStore.State;
                SendBtn.IsEnabled = state.State == ConnectionState.Connected && _selectedThreadId != null;
            }
        }

        private void OnPromptKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                var shiftDown = (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
                if (!shiftDown)
                {
                    OnSendPrompt(sender, e);
                    e.Handled = true;
                }
            }
        }

        private async void OnInterrupt(object sender, RoutedEventArgs e)
        {
            if (_selectedThreadId == null) return;

            try
            {
                await App.Services.OrchestrationService.DispatchTurnInterruptAsync(_selectedThreadId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Interrupt failed: {ex.Message}");
            }
        }

        private async void OnNewThread(object sender, RoutedEventArgs e)
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null || snapshot.Projects.Count == 0) return;

            // Use selected project, or fall back to first project if none selected
            var project = _selectedProjectId != null
                ? snapshot.Projects.FirstOrDefault(p => p.Id.Value == _selectedProjectId)
                : null;
            project ??= snapshot.Projects[0];

            var threadId = Guid.NewGuid().ToString();
            var title = $"Thread {DateTime.Now:yyyy-MM-dd HH:mm}";

            try
            {
                await App.Services.OrchestrationService.DispatchCreateThreadAsync(
                    project.Id.Value, threadId, title);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create thread failed: {ex.Message}");
            }
        }

        private void OnViewDiff(object sender, RoutedEventArgs e)
        {
            Frame?.Navigate(typeof(DiffPage));
        }

        private static string StatusToDisplay(ThreadStatus status) => status switch
        {
            ThreadStatus.Idle => "Idle",
            ThreadStatus.Starting => "Starting...",
            ThreadStatus.Running => "Running...",
            ThreadStatus.Interrupted => "Interrupted",
            ThreadStatus.Ready => "Ready",
            ThreadStatus.Stopped => "Stopped",
            ThreadStatus.Error => "Error",
            _ => "Unknown",
        };
    }
}
