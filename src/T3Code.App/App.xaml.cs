using Microsoft.UI.Xaml.Navigation;
using T3Code.BackendHost;

namespace T3Code.App
{
    public partial class App : Application
    {
        private Window window = Window.Current;

        public static AppServices Services { get; private set; } = null!;

        public App()
        {
            Services = new AppServices();
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            window ??= new Window();

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                window.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(Views.ShellPage), args.Arguments);
            window.Activate();

            _ = StartBackendAndConnectAsync();
        }

        private async Task StartBackendAndConnectAsync()
        {
            try
            {
                var backendPath = ResolveBackendPath();
                Services.BackendHost.Configure(new BackendConfig
                {
                    ExecutablePath = backendPath,
                    Cwd = Services.Paths.DataRoot,
                    Port = 0,
                    AuthToken = ProcessSupervisor.GenerateAuthToken(),
                    T3Home = Services.Paths.DataRoot,
                    Mode = "desktop",
                    NoBrowser = true,
                });

                var connectionInfo = await Services.BackendHost.StartAsync();
                await Services.TransportClient.ConnectAsync(connectionInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backend start failed: {ex.Message}");
            }
        }

        private static string ResolveBackendPath()
        {
            var appDir = AppContext.BaseDirectory;
            var serverDir = Path.Combine(appDir, "server");

            if (Directory.Exists(serverDir))
            {
                var binFile = Path.Combine(serverDir, "bin.mjs");
                if (File.Exists(binFile))
                {
                    return binFile;
                }
            }

            return "node";
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            throw new InvalidOperationException($"Failed to load Page {args.SourcePageType.FullName}");
        }
    }
}
