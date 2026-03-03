using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace open_health_windows.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        public async void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri("https://github.com/dombi-balazs/open-health-windows");
                bool success = await Windows.System.Launcher.LaunchUriAsync(uri);

                if (!success)
                {
                    ShowInfoBar("Warning", "Unable to open the GitHub page in the browser.", InfoBarSeverity.Warning);
                }
            }
            catch (UriFormatException)
            {
                ShowInfoBar("Error", "The provided URL format is invalid.", InfoBarSeverity.Error);
            }
            catch (Exception ex)
            {
                ShowInfoBar("Unexpected Error", $"An error occurred during the operation: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private void ShowInfoBar(string title, string message, InfoBarSeverity severity)
        {
            AppInfoBar.Title = title;
            AppInfoBar.Message = message;
            AppInfoBar.Severity = severity;
            AppInfoBar.IsOpen = true;
        }
    }
}