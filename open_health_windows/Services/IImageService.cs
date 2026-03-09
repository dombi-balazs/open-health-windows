using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using open_health_windows.Entities;

namespace open_health_windows.Services
{
    internal interface IImageService
    {
        Task<ImageEntity?> LoadImageAsync(IntPtr windowHandle);

    }
}
