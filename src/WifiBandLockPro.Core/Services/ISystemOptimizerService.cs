// Standardized to production level
// Purpose: Interface defining RAM optimization, process monitoring, and safe system junk cleaning operations
// Dependencies: System.Collections.Generic, System.Threading.Tasks, WifiBandLockPro.Core.Models

namespace WifiBandLockPro.Core.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using WifiBandLockPro.Core.Models;

public interface ISystemOptimizerService
{
    Task<SystemMemoryStatus> GetMemoryStatusAsync();
    Task<List<ProcessMemoryItem>> GetTopProcessesAsync(int limit = 50);
    Task<bool> EndProcessTaskAsync(int processId);
    Task<int> OptimizeRAMAsync();
    Task<JunkScanResult> ScanSafeJunkAsync();
    Task<JunkScanResult> CleanSafeJunkAsync();
    Task<string> RunUltraCompressModeAsync();
}
