using System.Collections.Generic;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal interface IHardwareService
    {
        Task InitializeAIPCHardwareAsync();

        List<string> GetAvailableExecutionProviders();

        bool IsHardwareAvailable(string providerName);
    }
}