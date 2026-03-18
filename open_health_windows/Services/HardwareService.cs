using Microsoft.Windows.AI.MachineLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal class HardwareService : IHardwareService
    {
        public async Task InitializeAIPCHardwareAsync()
        {
            try
            {
                var catalog = ExecutionProviderCatalog.GetDefault();
                await catalog.EnsureAndRegisterCertifiedAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error occured while initializing hardware: {ex.Message}");
            }
        }

        public List<string> GetAvailableExecutionProviders()
        {
            return ExecutionProviderCatalog.GetDefault()
                .FindAllProviders()
                .Where(p => p.ReadyState == ExecutionProviderReadyState.Ready)
                .Select(p => p.Name)
                .ToList();
        }

        public bool IsHardwareAvailable(string providerName)
        {
            var providers = GetAvailableExecutionProviders();
            return providers.Any(p => p.Contains(providerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}