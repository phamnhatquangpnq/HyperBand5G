// Standardized to production level
// Purpose: Interface for persistence of application settings and Windows Startup registry management
// Dependencies: System.Threading.Tasks, HyperBoost.Core.Models

namespace HyperBoost.Core.Services;

using System.Threading.Tasks;
using HyperBoost.Core.Models;

public interface ISettingsService
{
    Task<AppSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
    void ApplyWindowsStartup(bool enable);
}
