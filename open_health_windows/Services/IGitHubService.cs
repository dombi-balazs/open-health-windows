using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal interface IGitHubService
    {
        string GitHubAddress { get; }

        Task<bool> LaunchRepositoryAsync();

    }
}
