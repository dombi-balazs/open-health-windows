using open_health_windows.Entities;
using System;
using System.IO; // A Stream-ekhez kell
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace open_health_windows.Services
{
    internal class ImageService : IImageService
    {
        public async Task<ImageEntity?> LoadImageAsync(IntPtr windowHandle)
        {
            try
            {
                FileOpenPicker open = new FileOpenPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(open, windowHandle);

                open.FileTypeFilter.Add(".jpg");
                open.FileTypeFilter.Add(".jpeg");
                open.FileTypeFilter.Add(".png");
                open.FileTypeFilter.Add(".bmp");

                open.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

                StorageFile file = await open.PickSingleFileAsync();

                if (file != null)
                {
                    using var stream = await file.OpenStreamForReadAsync();
                    using var memoryStream = new MemoryStream();

                    await stream.CopyToAsync(memoryStream);
                    return new ImageEntity
                    {
                        ImagePath = file.Path,
                        ImageByteArray = memoryStream.ToArray()
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            }

            return null;
        }
    }
}