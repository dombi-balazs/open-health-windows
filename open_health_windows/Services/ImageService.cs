using open_health_windows.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;

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
            public async Task<IReadOnlyList<StorageFile>?> LoadFolderAsync(IntPtr windowHandle)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, windowHandle);

                folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                folderPicker.FileTypeFilter.Add("*");

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();

                if (folder != null)
                {
                    var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new[] { ".jpg", ".jpeg", ".png", ".bmp" });
                    var query = folder.CreateFileQueryWithOptions(queryOptions);
                    return await query.GetFilesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while loading folder: {ex.Message}");
            }

            return null;
        }
    }
}