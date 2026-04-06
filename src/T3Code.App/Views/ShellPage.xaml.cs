using Microsoft.UI.Xaml.Navigation;

namespace T3Code.App.Views
{
    public partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(MainPage));
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.InvokedItemContainer?.Tag as string;
            if (tag == null) return;

            var pageType = tag switch
            {
                "chats" => typeof(MainPage),
                _ => null,
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }
}
