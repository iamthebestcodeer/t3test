using T3Code.Core.Models;

namespace T3Code.App.Views
{
    public partial class SettingsPage : Page
    {
        private Action<ServerSettings>? _settingsChangedHandler;
        private Action<DesktopUpdateState>? _updateStateChangedHandler;
        private Action<ConnectionStateSnapshot>? _connectionStateChangedHandler;

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSettings();
            SubscribeToEvents();
            this.Unloaded += (_, _) => UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _settingsChangedHandler = _ => DispatcherQueue.TryEnqueue(LoadSettings);
            App.Services.SettingsStore.SettingsChanged += _settingsChangedHandler;

            _updateStateChangedHandler = state => DispatcherQueue.TryEnqueue(() =>
            {
                if (state != null)
                {
                    UpdateStatusText.Text = state.Status.ToString();
                }
            });
            App.Services.SettingsStore.UpdateStateChanged += _updateStateChangedHandler;

            _connectionStateChangedHandler = _ => DispatcherQueue.TryEnqueue(UpdateBackendInfo);
            App.Services.ConnectionStore.StateChanged += _connectionStateChangedHandler;
        }

        private void UnsubscribeFromEvents()
        {
            if (_settingsChangedHandler != null)
                App.Services.SettingsStore.SettingsChanged -= _settingsChangedHandler;
            if (_updateStateChangedHandler != null)
                App.Services.SettingsStore.UpdateStateChanged -= _updateStateChangedHandler;
            if (_connectionStateChangedHandler != null)
                App.Services.ConnectionStore.StateChanged -= _connectionStateChangedHandler;
        }

        private void LoadSettings()
        {
            var settings = App.Services.SettingsStore.Settings;
            if (settings != null)
            {
                // Set default mode combo
                SetComboBoxByTag(DefaultModeSelector, settings.DefaultRuntimeMode ?? "Desktop");
                SetComboBoxByTag(InteractionSelector, settings.DefaultInteractionMode ?? "Default");
            }

            // Set theme combo
            var currentTheme = App.Services.ThemeService.CurrentTheme;
            SetComboBoxByTag(ThemeSelector, currentTheme.ToString());

            UpdateBackendInfo();

            var updateState = App.Services.SettingsStore.UpdateState;
            if (updateState != null)
            {
                UpdateStatusText.Text = updateState.Status.ToString();
            }
        }

        private void UpdateBackendInfo()
        {
            var pid = App.Services.BackendHost.BackendPid;
            BackendPidText.Text = pid > 0 ? pid.ToString() : "N/A";
            RestartBackendBtn.IsEnabled = pid > 0;
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is not ComboBoxItem item) return;
            if (Enum.TryParse<Theme>(item.Tag?.ToString(), out var theme))
            {
                _ = App.Services.ThemeService.SetThemeAsync(theme);
            }
        }

        private async void OnDefaultModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultModeSelector.SelectedItem is not ComboBoxItem item) return;
            var mode = item.Tag?.ToString();
            if (mode == null) return;

            try
            {
                await App.Services.SettingsService.UpdateSettingsAsync(
                    new Dictionary<string, object> { ["defaultRuntimeMode"] = mode });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update settings: {ex.Message}");
            }
        }

        private async void OnInteractionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InteractionSelector.SelectedItem is not ComboBoxItem item) return;
            var mode = item.Tag?.ToString();
            if (mode == null) return;

            try
            {
                await App.Services.SettingsService.UpdateSettingsAsync(
                    new Dictionary<string, object> { ["defaultInteractionMode"] = mode });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update settings: {ex.Message}");
            }
        }

        private async void OnCheckUpdates(object sender, RoutedEventArgs e)
        {
            CheckUpdatesBtn.IsEnabled = false;
            UpdateStatusText.Text = "Checking...";

            try
            {
                await App.Services.UpdateService.CheckForUpdatesAsync();
                UpdateStatusText.Text = App.Services.UpdateService.CurrentState.Status.ToString();
            }
            catch (Exception ex)
            {
                UpdateStatusText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                CheckUpdatesBtn.IsEnabled = true;
            }
        }

        private async void OnRestartBackend(object sender, RoutedEventArgs e)
        {
            RestartBackendBtn.IsEnabled = false;
            BackendPidText.Text = "Restarting...";

            try
            {
                var connectionInfo = await App.Services.BackendHost.RestartAsync();
                if (connectionInfo != null)
                {
                    await App.Services.TransportClient.ConnectAsync(connectionInfo);
                    await App.Services.OrchestrationService.SubscribeToEventsAsync();
                    await App.Services.TerminalService.SubscribeToEventsAsync();
                }
            }
            catch (Exception ex)
            {
                BackendPidText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                UpdateBackendInfo();
            }
        }

        private void OnViewLogs(object sender, RoutedEventArgs e)
        {
            var logsDir = App.Services.Paths.LogsDir;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logsDir,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open logs folder: {ex.Message}");
            }
        }

        private static void SetComboBoxByTag(ComboBox combo, string tag)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
