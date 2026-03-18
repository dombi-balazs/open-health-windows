using System;

namespace open_health_windows.Entities
{
    internal class ImageEntity
    {
        public string? ImagePath { get; set; }
        public byte[] ImageByteArray { get; set; } = Array.Empty<byte>();
    }
}