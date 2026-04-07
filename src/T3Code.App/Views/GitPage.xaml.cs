using Microsoft.UI.Xaml.Media;
using T3Code.Core.Models;

namespace T3Code.App.Views
{
    public sealed class GitFileViewModel
    {
        public required string Path { get; init; }
        public required string StatusLabel { get; init; }
        public required string StatusColor { get; init; }
        public required string FileGlyph { get; init; }
    }

    public sealed class GitProjectItem
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Cwd { get; init; }
    }

    public partial class GitPage : Page
    {
        private readonly List<GitProjectItem> _projects = [];
        private string? _selectedCwd;
        private Action<OrchestrationSnapshot?>? _snapshotChangedHandler;
        private Action<GitStatus>? _statusUpdatedHandler;

        public GitPage()
        {
            this.InitializeComponent();
            SubscribeToEvents();
            LoadProjects();
            this.Unloaded += (_, _) => UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _snapshotChangedHandler = _ => DispatcherQueue.TryEnqueue(LoadProjects);
            App.Services.OrchestrationStore.SnapshotChanged += _snapshotChangedHandler;

            _statusUpdatedHandler = status => DispatcherQueue.TryEnqueue(() => RenderStatus(status));
            App.Services.GitStore.StatusUpdated += _statusUpdatedHandler;
        }

        private void UnsubscribeFromEvents()
        {
            if (_snapshotChangedHandler != null)
                App.Services.OrchestrationStore.SnapshotChanged -= _snapshotChangedHandler;
            if (_statusUpdatedHandler != null)
                App.Services.GitStore.StatusUpdated -= _statusUpdatedHandler;
        }

        private void LoadProjects()
        {
            var snapshot = App.Services.OrchestrationStore.Snapshot;
            if (snapshot == null) return;

            _projects.Clear();
            foreach (var p in snapshot.Projects)
            {
                _projects.Add(new GitProjectItem
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
            if (ProjectSelector.SelectedItem is not GitProjectItem selected)
                return;

            _selectedCwd = selected.Cwd;
            _ = RefreshGitStatusAsync();
        }

        private async void OnRefreshStatus(object sender, RoutedEventArgs e)
        {
            await RefreshGitStatusAsync();
        }

        private async void OnListBranches(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCwd)) return;

            try
            {
                var branches = await App.Services.GitService.ListBranchesAsync(_selectedCwd);
                var sb = new System.Text.StringBuilder();
                foreach (var b in branches)
                {
                    var prefix = b.IsCurrent ? "* " : "  ";
                    sb.AppendLine($"{prefix}{b.Name}");
                    if (!string.IsNullOrEmpty(b.Upstream))
                    {
                        sb.AppendLine($"    upstream: {b.Upstream}");
                    }
                }

                FileStatusList.ItemsSource = new List<GitFileViewModel>
                {
                    new()
                    {
                        Path = sb.Length > 0 ? sb.ToString().TrimEnd() : "No branches found.",
                        StatusLabel = "INFO",
                        StatusColor = "Gray",
                        FileGlyph = "\uE8A5",
                    },
                };
            }
            catch (Exception ex)
            {
                BranchName.Text = $"Error: {ex.Message}";
            }
        }

        private async void OnPull(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCwd)) return;

            try
            {
                var result = await App.Services.GitService.PullAsync(_selectedCwd);
                BranchName.Text = "Pull complete.";
                await RefreshGitStatusAsync();
            }
            catch (Exception ex)
            {
                BranchName.Text = $"Pull failed: {ex.Message}";
            }
        }

        private async void OnInitRepo(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCwd)) return;

            try
            {
                await App.Services.GitService.InitAsync(_selectedCwd);
                await RefreshGitStatusAsync();
            }
            catch (Exception ex)
            {
                BranchName.Text = $"Init failed: {ex.Message}";
            }
        }

        private async Task RefreshGitStatusAsync()
        {
            if (string.IsNullOrEmpty(_selectedCwd)) return;

            try
            {
                await App.Services.GitService.RefreshStatusAsync(_selectedCwd);
            }
            catch (Exception ex)
            {
                BranchName.Text = $"Error: {ex.Message}";
            }
        }

        private void RenderStatus(GitStatus status)
        {
            BranchName.Text = status.Branch ?? "(no branch)";

            var aheadBehind = new System.Text.StringBuilder();
            if (status.Ahead > 0) aheadBehind.Append($"ahead {status.Ahead} ");
            if (status.Behind > 0) aheadBehind.Append($"behind {status.Behind}");
            AheadBehindText.Text = aheadBehind.ToString().Trim();

            var files = status.Files.Select(f => new GitFileViewModel
            {
                Path = f.Path,
                StatusLabel = StatusToLabel(f.Status),
                StatusColor = StatusToColor(f.Status),
                FileGlyph = StatusToGlyph(f.Status),
            }).ToList();

            FileStatusList.ItemsSource = files;
        }

        private static string StatusToLabel(GitFileStatus status) => status switch
        {
            GitFileStatus.Modified => "M",
            GitFileStatus.Added => "A",
            GitFileStatus.Deleted => "D",
            GitFileStatus.Renamed => "R",
            GitFileStatus.Copied => "C",
            GitFileStatus.Untracked => "U",
            GitFileStatus.Ignored => "I",
            GitFileStatus.Unmerged => "?",
            _ => "?",
        };

        private static string StatusToColor(GitFileStatus status) => status switch
        {
            GitFileStatus.Modified => "#E2A600",
            GitFileStatus.Added => "#2EA043",
            GitFileStatus.Deleted => "#D73A49",
            GitFileStatus.Renamed => "#0078D4",
            GitFileStatus.Copied => "#0078D4",
            GitFileStatus.Untracked => "#6E7681",
            GitFileStatus.Ignored => "#6E7681",
            GitFileStatus.Unmerged => "#D73A49",
            _ => "#6E7681",
        };

        private static string StatusToGlyph(GitFileStatus status) => status switch
        {
            GitFileStatus.Modified => "\uE7BA",
            GitFileStatus.Added => "\uE710",
            GitFileStatus.Deleted => "\uE74D",
            GitFileStatus.Renamed => "\uE8B9",
            _ => "\uE7C3",
        };
    }
}
