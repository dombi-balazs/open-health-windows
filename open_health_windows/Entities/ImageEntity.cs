using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_health_windows.Entities
{
    internal class ImageEntity
    {
        public string? ImagePath { get; set; }
        public byte[] ImageByteArray { get; set; } = Array.Empty<byte>();
    }
}