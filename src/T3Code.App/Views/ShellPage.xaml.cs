namespace T3Code.App.Views
{
    public partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(MainPage));
            NavView.ItemInvoked += OnItemInvoked;
            NavView.BackRequested += OnBackRequested;
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                if (ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
                {
                    ContentFrame.Navigate(typeof(SettingsPage));
                }

                return;
            }

            var tag = args.InvokedItemContainer?.Tag as string;
            if (tag == null)
            {
                return;
            }

            var pageType = tag switch
            {
                "chats" => typeof(MainPage),
                "terminal" => typeof(TerminalPage),
                "diff" => typeof(DiffPage),
                "git" => typeof(GitPage),
                _ => null,
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }
}
