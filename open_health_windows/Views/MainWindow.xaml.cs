using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using open_health_windows.Entities;
using open_health_windows.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace open_health_windows.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly IGitHubService _gitHubService;
        private readonly IImageService _imageService;
        private readonly IHardwareService _hardwareService;
        private readonly EvaluationService _evaluationService;

        private ImageEntity? _currentLoadedImage;
        private IReadOnlyList<StorageFile>? _folderFiles;
        private List<AnalysisResultEntity> _analysisResults = new();
        private ChosenHardwareEntity.HardwareChoice _selectedHardware = ChosenHardwareEntity.HardwareChoice.CPU;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            _gitHubService = new GitHubService();
            _imageService = new ImageService();
            _hardwareService = new HardwareService();
            _evaluationService = new EvaluationService();

            Task.Run(async () => await _hardwareService.InitializeAIPCHardwareAsync());

            AnalysisProgressBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 144, 255));
        }

        private void HardwareMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string hwStr)
            {
                if (Enum.TryParse(hwStr, out ChosenHardwareEntity.HardwareChoice choice))
                {
                    _selectedHardware = choice;
                    SelectedHardwareText.Text = $"Hardware: {choice}";
                }
            }
        }

        public async void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _currentLoadedImage = await _imageService.LoadImageAsync(windowHandle);

            if (_currentLoadedImage != null && _currentLoadedImage.ImageByteArray.Length > 0)
            {
                _folderFiles = null;
                await DisplayImageAsync(_currentLoadedImage.ImageByteArray);
                ProgressText.Text = "Image loaded.";
                ShowInfoBar("Success", "Image loaded successfully.", InfoBarSeverity.Success);
            }
        }

        public async void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _folderFiles = await _imageService.LoadFolderAsync(windowHandle);

            if (_folderFiles != null && _folderFiles.Count > 0)
            {
                _currentLoadedImage = null;
                LoadedImage.Source = null;
                ProgressText.Text = $"Loaded folder with {_folderFiles.Count} images.";
                ShowInfoBar("Folder Loaded", $"Found {_folderFiles.Count} image files.", InfoBarSeverity.Success);
            }
        }

        public async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLoadedImage == null && (_folderFiles == null || _folderFiles.Count == 0))
            {
                ShowInfoBar("Warning", "Please load an image or folder first.", InfoBarSeverity.Warning);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            StopButton.IsEnabled = true;
            AnalysisProgressBar.Visibility = Visibility.Visible;
            _analysisResults.Clear();

            try
            {
                string modelPath1 = Path.Combine(AppContext.BaseDirectory, "Assets", "AiModels", "best.onnx");
                string modelPath2 = Path.Combine(AppContext.BaseDirectory, "Assets", "AiModels", "quantized_skin_analyzer_model.onnx");

                bool useQuantized = QuantizedModelRadio.IsChecked ?? false;
                string activeModel = useQuantized ? "INT8 Quantized" : "FP32 Original";

                ProgressText.Text = $"Initializing {activeModel} model...";

                await Task.Run(() => _evaluationService.InitializeModels(_selectedHardware, modelPath1, modelPath2));

                _evaluationService.SetModelMode(useQuantized);

                if (_folderFiles != null)
                {
                    AnalysisProgressBar.IsIndeterminate = false;
                    AnalysisProgressBar.Minimum = 0;
                    AnalysisProgressBar.Maximum = _folderFiles.Count;
                    AnalysisProgressBar.Value = 0;

                    for (int i = 0; i < _folderFiles.Count; i++)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        var file = _folderFiles[i];
                        ProgressText.Text = $"[{activeModel}] Analyzing {i + 1}/{_folderFiles.Count}: {file.Name}";

                        using var stream = await file.OpenStreamForReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        byte[] bytes = ms.ToArray();

                        await DisplayImageAsync(bytes);
                        string result = await _evaluationService.AnalyzeImageAsync(bytes);

                        _analysisResults.Add(new AnalysisResultEntity
                        {
                            FileName = file.Name,
                            AnalysisTime = DateTime.Now,
                            Result = $"{activeModel} | {result}"
                        });

                        AnalysisProgressBar.Value = i + 1;
                    }
                    ProgressText.Text = "Folder analysis complete.";
                }
                else if (_currentLoadedImage != null)
                {
                    AnalysisProgressBar.IsIndeterminate = true;
                    ProgressText.Text = $"Running {activeModel} inference...";

                    string result = await _evaluationService.AnalyzeImageAsync(_currentLoadedImage.ImageByteArray);
                    ProgressText.Text = result;

                    _analysisResults.Add(new AnalysisResultEntity
                    {
                        FileName = Path.GetFileName(_currentLoadedImage.ImagePath ?? "Unknown"),
                        AnalysisTime = DateTime.Now,
                        Result = $"{activeModel} | {result}"
                    });
                }
            }
            catch (OperationCanceledException)
            {
                ProgressText.Text = "Analysis stopped.";
            }
            catch (Exception ex)
            {
                ProgressText.Text = "Error.";
                ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                StopButton.IsEnabled = false;
                AnalysisProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            StopButton.IsEnabled = false;
        }

        public async void SaveResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (_analysisResults.Count == 0) return;

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            FileSavePicker savePicker = new FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = $"Analysis_{DateTime.Now:yyyyMMdd_HHmm}";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var lines = new List<string> { "Analysis Report", $"Hardware: {_selectedHardware}", "---" };
                lines.AddRange(_analysisResults.Select(r => $"[{r.AnalysisTime}] {r.FileName}: {r.Result}"));
                await FileIO.WriteLinesAsync(file, lines);
                ShowInfoBar("Saved", "Results saved.", InfoBarSeverity.Success);
            }
        }

        public async void GitHubButton_Click(object sender, RoutedEventArgs e) => await _gitHubService.LaunchRepositoryAsync();

        private async Task DisplayImageAsync(byte[] imageBytes)
        {
            DispatcherQueue.TryEnqueue(async () => {
                BitmapImage bitmapImage = new BitmapImage();
                using var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(imageBytes.AsBuffer());
                stream.Seek(0);
                await bitmapImage.SetSourceAsync(stream);
                LoadedImage.Source = bitmapImage;
            });
        }

        private void ShowInfoBar(string title, string message, InfoBarSeverity severity)
        {
            AppInfoBar.Title = title; AppInfoBar.Message = message; AppInfoBar.Severity = severity; AppInfoBar.IsOpen = true;
        }
    }
}