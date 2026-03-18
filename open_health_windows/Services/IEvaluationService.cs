using open_health_windows.Entities;
using System;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal interface IEvaluationService : IDisposable
    {
        void InitializeModels(ChosenHardwareEntity.HardwareChoice hardwareChoice, string model1Path, string model2Path);
        void SetModelMode(bool useQuantized);
        Task<string> AnalyzeImageAsync(byte[] imageBytes);
    }
}