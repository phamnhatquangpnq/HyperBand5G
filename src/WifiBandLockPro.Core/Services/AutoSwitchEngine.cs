// Standardized to production level
// Purpose: Autonomous engine that monitors Wi-Fi band state and triggers automatic switching from 2.4GHz to 5GHz BSSIDs
// Dependencies: System, WifiBandLockPro.Core.Models, WifiBandLockPro.Core.Services

namespace WifiBandLockPro.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WifiBandLockPro.Core.Models;

public class AutoSwitchEngine
{
    private readonly IWiFiService _wifiService;
    public AutoSwitchConfig Config { get; set; }
    public List<SwitchEventLog> EventLogs { get; } = new();

    public event EventHandler<SwitchEventLog>? OnSwitchEvent;

    public AutoSwitchEngine(IWiFiService wifiService, AutoSwitchConfig? config = null)
    {
        _wifiService = wifiService ?? throw new ArgumentNullException(nameof(wifiService));
        Config = config ?? new AutoSwitchConfig(
            Enabled: true,
            Mode: AutoSwitchMode.Smart,
            PollIntervalMs: 3000,
            Min5GhzQualityThreshold: 35,
            NotifyOnSwitch: true,
            Prefer5GhzAdapterProperty: true
        );
    }

    /**
     * Evaluates the current Wi-Fi connection state.
     * If connected to 2.4 GHz, scans for matching 5 GHz BSSID and triggers an automatic switch if threshold is met.
     */
    public virtual async Task<SwitchEventLog?> EvaluateAndSwitchAsync()
    {
        if (!Config.Enabled || Config.Mode == AutoSwitchMode.Disabled)
            return null;

        var current = await _wifiService.GetCurrentInterfaceAsync();
        if (current == null || !string.Equals(current.State, "connected", StringComparison.OrdinalIgnoreCase))
            return null;

        if (current.Band == WiFiBand.Band5GHz || current.Band == WiFiBand.Band6GHz)
            return null;

        if (current.Band == WiFiBand.Band24GHz)
        {
            var bssids = await _wifiService.ScanBSSIDsAsync(current.Bssid);
            
            // Find best 5GHz or 6GHz BSSID for the SAME SSID
            var best5Ghz = bssids
                .Where(b => string.Equals(b.Ssid, current.Ssid, StringComparison.OrdinalIgnoreCase) &&
                            (b.Band == WiFiBand.Band5GHz || b.Band == WiFiBand.Band6GHz) &&
                            b.SignalQuality >= Config.Min5GhzQualityThreshold)
                .OrderByDescending(b => b.Score)
                .FirstOrDefault();

            if (best5Ghz != null)
            {
                bool success = await _wifiService.ConnectToBSSIDAsync(best5Ghz.Ssid, best5Ghz.Bssid);
                if (success && Config.Prefer5GhzAdapterProperty)
                {
                    await _wifiService.SetAdapterPreferredBand5GHzAsync();
                }

                var log = new SwitchEventLog(
                    Id: Guid.NewGuid().ToString("N"),
                    Timestamp: DateTime.Now,
                    Ssid: current.Ssid,
                    FromBssid: current.Bssid,
                    FromBand: current.Band,
                    ToBssid: best5Ghz.Bssid,
                    ToBand: best5Ghz.Band,
                    Reason: $"Detected 2.4 GHz connection on {current.Ssid}. Auto-switched to 5 GHz BSSID {best5Ghz.Bssid} (Signal: {best5Ghz.SignalQuality}%, Score: {best5Ghz.Score}).",
                    Success: success
                );

                EventLogs.Add(log);
                OnSwitchEvent?.Invoke(this, log);
                return log;
            }
            else if (Config.Mode == AutoSwitchMode.Strict)
            {
                // In strict mode, if no 5GHz is available, log warning
            }
        }

        return null;
    }
}
