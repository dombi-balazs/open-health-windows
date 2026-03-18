using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using open_health_windows.Entities;

namespace open_health_windows.Services
{
    internal interface IImageService
    {
        Task<ImageEntity?> LoadImageAsync(IntPtr windowHandle);
        Task<IReadOnlyList<StorageFile>?> LoadFolderAsync(IntPtr windowHandle);
    }
}