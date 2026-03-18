using System;

namespace open_health_windows.Entities
{
    public class AnalysisResultEntity
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime AnalysisTime { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}