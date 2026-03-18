using System;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal class GitHubService : IGitHubService
    {
        public string GitHubAddress => "https://github.com/dombi-balazs/open-health-windows";

        public async Task<bool> LaunchRepositoryAsync()
        {
            var uri = new Uri(GitHubAddress);
            return await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
