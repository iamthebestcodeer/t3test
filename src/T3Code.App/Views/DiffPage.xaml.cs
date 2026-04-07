using T3Code.Core.Models;

namespace T3Code.App.Views
{
    public sealed class DiffThreadItem
    {
        public required string ThreadId { get; init; }
        public required string Title { get; init; }
        public string? TurnId { get; init; }
    }

    public partial class DiffPage : Page
    {
        private readonly List<DiffThreadItem> _threads = [];
        private Action<OrchestrationSnapshot?>? _snapshotChangedHandler;

        public DiffPage()
        {
            this.InitializeComponent();
            SubscribeToEvents();
            LoadThreads();
            this.Unloaded += (_, _) => UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _snapshotChangedHandler = _ => DispatcherQueue.TryEnqueue(LoadThreads);
            App.Services.OrchestrationStore.SnapshotChanged += _snapshotChangedHandler;
        }

        private void UnsubscribeFromEvents()
        {
            if (_snapshotChangedHandler != null)
            {
                App.Services.OrchestrationStore.SnapshotChanged -= _snapshotChangedHandler;
            }
        }

        private void LoadThreads()
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null) return;

            _threads.Clear();
            foreach (var t in snapshot.Threads)
            {
                _threads.Add(new DiffThreadItem
                {
                    ThreadId = t.Id.Value,
                    Title = t.Title,
                    TurnId = t.CurrentTurnId?.Value,
                });
            }

            ThreadSelector.ItemsSource = null;
            ThreadSelector.ItemsSource = _threads;
            ThreadSelector.DisplayMemberPath = "Title";
        }

        private void OnThreadSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ThreadSelector.SelectedItem is DiffThreadItem selected)
            {
                LoadTurnDiffBtn.IsEnabled = true;
                LoadFullDiffBtn.IsEnabled = true;
                DiffSummaryText.Text = $"Thread: {selected.Title}";
            }
            else
            {
                LoadTurnDiffBtn.IsEnabled = false;
                LoadFullDiffBtn.IsEnabled = false;
            }
        }

        private async void OnLoadTurnDiff(object sender, RoutedEventArgs e)
        {
            if (ThreadSelector.SelectedItem is not DiffThreadItem selected)
                return;

            if (string.IsNullOrEmpty(selected.TurnId))
            {
                DiffContent.Text = "No turn to diff for this thread.";
                return;
            }

            try
            {
                DiffContent.Text = "Loading turn diff...";
                var result = await App.Services.OrchestrationService.GetTurnDiffAsync(selected.TurnId);
                RenderDiff(result.Files);
            }
            catch (Exception ex)
            {
                DiffContent.Text = $"Failed to load turn diff: {ex.Message}";
            }
        }

        private async void OnLoadFullDiff(object sender, RoutedEventArgs e)
        {
            if (ThreadSelector.SelectedItem is not DiffThreadItem selected)
                return;

            try
            {
                DiffContent.Text = "Loading full thread diff...";
                var result = await App.Services.OrchestrationService.GetFullThreadDiffAsync(selected.ThreadId);
                RenderDiff(result.Files);
            }
            catch (Exception ex)
            {
                DiffContent.Text = $"Failed to load full diff: {ex.Message}";
            }
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            LoadThreads();
        }

        private void RenderDiff(IReadOnlyList<DiffFileEntry> files)
        {
            if (files.Count == 0)
            {
                DiffContent.Text = "No changes found.";
                DiffSummaryText.Text = "0 files changed";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- {files.Count} file(s) changed ---");
            sb.AppendLine();

            foreach (var file in files)
            {
                sb.AppendLine($"=== {file.Status.ToUpperInvariant()}: {file.Path} ===");
                if (!string.IsNullOrEmpty(file.DiffContent))
                {
                    sb.AppendLine(file.DiffContent);
                }
                else
                {
                    sb.AppendLine($"[{file.Status}]");
                }
                sb.AppendLine();
            }

            DiffContent.Text = sb.ToString();
            DiffSummaryText.Text = $"{files.Count} file(s) changed";
        }
    }
}
