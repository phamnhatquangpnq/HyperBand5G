// Standardized to production level
// Purpose: Interface for Wi-Fi service operations to enable clean dependency injection and unit testing
// Dependencies: HyperBoost.Core.Models

namespace HyperBoost.Core.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using HyperBoost.Core.Models;

public interface IWiFiService
{
    WiFiBand GetBandFromChannel(int channel);
    WiFiInterfaceStatus? ParseInterfaceStatus(string rawOutput);
    List<BSSIDNetwork> ParseBSSIDNetworks(string rawOutput, string? currentBssid = null);
    Task<WiFiInterfaceStatus?> GetCurrentInterfaceAsync();
    Task<List<BSSIDNetwork>> ScanBSSIDsAsync(string? currentBssid = null);
    Task<bool> ConnectToBSSIDAsync(string ssid, string bssid);
    Task<bool> SetAdapterPreferredBand5GHzAsync();
    Task<bool> ResetAdapterPreferredBandAsync();
}
