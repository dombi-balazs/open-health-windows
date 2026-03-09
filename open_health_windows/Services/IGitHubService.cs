using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal interface IGitHubService
    {
        string GitHubAddress { get; }

        Task<bool> LaunchRepositoryAsync();


    }
}
