using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using open_health_windows.Entities;
using open_health_windows.Services;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace open_health_windows.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly IGitHubService _gitHubService;
        private ImageEntity? _currentLoadedImage;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            _gitHubService = new GitHubService();
        }

        public async void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _gitHubService.LaunchRepositoryAsync();
                ShowInfoBar("Success", "Opened the GitHub page in the browser.", InfoBarSeverity.Success);

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

        public async void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            var imageService = new ImageService();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            _currentLoadedImage = await imageService.LoadImageAsync(windowHandle);

            if (_currentLoadedImage != null && _currentLoadedImage.ImageByteArray.Length > 0)
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelWidth = 600;

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(_currentLoadedImage.ImageByteArray.AsBuffer());
                    stream.Seek(0);
                    await bitmapImage.SetSourceAsync(stream);
                }

                LoadedImage.Source = bitmapImage;
                ShowInfoBar("Success", "Image loaded and converted to bytes successfully.", InfoBarSeverity.Success);
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