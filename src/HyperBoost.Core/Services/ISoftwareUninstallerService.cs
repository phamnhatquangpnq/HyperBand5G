using System.Collections.Generic;
using System.Threading.Tasks;
using HyperBoost.Core.Models;

namespace HyperBoost.Core.Services;

public interface ISoftwareUninstallerService
{
	Task<List<InstalledAppItem>> GetInstalledAppsAsync();

	Task<bool> UninstallAppAsync(InstalledAppItem app);

	Task<bool> OpenInstallLocationAsync(InstalledAppItem app);
}
