// Standardized to production level
// Purpose: Core Wi-Fi service for executing Windows netsh commands, parsing BSSIDs, signal qualities, and enforcing 5GHz band connections
// Dependencies: System.Diagnostics, WifiBandLockPro.Core.Models

namespace WifiBandLockPro.Core.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WifiBandLockPro.Core.Models;

public class WiFiService : IWiFiService
{
    public WiFiBand GetBandFromChannel(int channel)
    {
        if (channel >= 1 && channel <= 14)
            return WiFiBand.Band24GHz;
        if (channel >= 36 && channel <= 177)
            return WiFiBand.Band5GHz;
        if (channel >= 181 || channel == 2 || (channel > 177 && channel <= 233))
            return WiFiBand.Band6GHz;
        return WiFiBand.Unknown;
    }

    public WiFiInterfaceStatus? ParseInterfaceStatus(string rawOutput)
    {
        if (string.IsNullOrWhiteSpace(rawOutput)) return null;

        var lines = rawOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var parts = line.Split(':', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var val = parts[1].Trim();
                dict[key] = val;
            }
        }

        if (!dict.ContainsKey("Name") && !dict.ContainsKey("SSID")) return null;

        string name = dict.GetValueOrDefault("Name", "Wi-Fi");
        string desc = dict.GetValueOrDefault("Description", "");
        string guid = dict.GetValueOrDefault("GUID", "");
        string mac = dict.GetValueOrDefault("Physical address", "");
        string state = dict.GetValueOrDefault("State", "unknown");
        string ssid = dict.GetValueOrDefault("SSID", "");
        string bssid = dict.GetValueOrDefault("BSSID", "");
        string netType = dict.GetValueOrDefault("Network type", "");
        string radio = dict.GetValueOrDefault("Radio type", "");
        
        int channel = 0;
        if (dict.TryGetValue("Channel", out var chStr)) int.TryParse(chStr, out channel);

        int rxRate = 0;
        if (dict.TryGetValue("Receive rate (Mbps)", out var rxStr)) int.TryParse(rxStr, out rxRate);

        int txRate = 0;
        if (dict.TryGetValue("Transmit rate (Mbps)", out var txStr)) int.TryParse(txStr, out txRate);

        int signalQuality = 0;
        if (dict.TryGetValue("Signal", out var sigStr))
        {
            sigStr = sigStr.Replace("%", "").Trim();
            int.TryParse(sigStr, out signalQuality);
        }

        int rssiDbm = (signalQuality / 2) - 100;
        WiFiBand band = GetBandFromChannel(channel);

        return new WiFiInterfaceStatus(
            name, desc, guid, mac, state, ssid, bssid, netType, radio,
            channel, rxRate, txRate, signalQuality, rssiDbm, band
        );
    }

    public List<BSSIDNetwork> ParseBSSIDNetworks(string rawOutput, string? currentBssid = null)
    {
        var result = new List<BSSIDNetwork>();
        if (string.IsNullOrWhiteSpace(rawOutput)) return result;

        var lines = rawOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string currentSsid = "";
        string authAlgo = "";
        string cipherAlgo = "";

        string? bssid = null;
        int signal = 0;
        string radioType = "";
        int channel = 0;

        void SaveCurrentBssid()
        {
            if (!string.IsNullOrEmpty(bssid))
            {
                var band = GetBandFromChannel(channel);
                int rssi = (signal / 2) - 100;
                bool isCurrent = !string.IsNullOrEmpty(currentBssid) && string.Equals(bssid, currentBssid, StringComparison.OrdinalIgnoreCase);
                
                // Score boost for 5GHz and Wi-Fi 6/5
                int bandBoost = band == WiFiBand.Band5GHz ? 20 : (band == WiFiBand.Band6GHz ? 30 : 0);
                int radioBoost = radioType.Contains("ax", StringComparison.OrdinalIgnoreCase) ? 10 : (radioType.Contains("ac", StringComparison.OrdinalIgnoreCase) ? 5 : 0);
                int score = Math.Min(100, signal + bandBoost + radioBoost);

                result.Add(new BSSIDNetwork(
                    currentSsid, bssid, signal, rssi, band, channel, radioType, authAlgo, cipherAlgo, score, isCurrent
                ));
            }
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var parts = trimmed.Split(':', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            var val = parts[1].Trim();

            if (key.StartsWith("SSID ", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "SSID", StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrentBssid();
                bssid = null;
                currentSsid = string.IsNullOrEmpty(val) ? "<Hidden Network>" : val;
            }
            else if (string.Equals(key, "Authentication", StringComparison.OrdinalIgnoreCase))
            {
                authAlgo = val;
            }
            else if (string.Equals(key, "Encryption", StringComparison.OrdinalIgnoreCase))
            {
                cipherAlgo = val;
            }
            else if (key.StartsWith("BSSID ", StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrentBssid();
                bssid = val;
                signal = 0;
                radioType = "";
                channel = 0;
            }
            else if (bssid != null)
            {
                if (string.Equals(key, "Signal", StringComparison.OrdinalIgnoreCase))
                {
                    val = val.Replace("%", "").Trim();
                    int.TryParse(val, out signal);
                }
                else if (string.Equals(key, "Radio type", StringComparison.OrdinalIgnoreCase))
                {
                    radioType = val;
                }
                else if (string.Equals(key, "Channel", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(val, out channel);
                }
            }
        }

        SaveCurrentBssid();
        return result;
    }

    public virtual async Task<WiFiInterfaceStatus?> GetCurrentInterfaceAsync()
    {
        try
        {
            var output = await RunCommandAsync("netsh", "wlan show interfaces");
            return ParseInterfaceStatus(output);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current interface: {ex.Message}");
            return null;
        }
    }

    public virtual async Task<List<BSSIDNetwork>> ScanBSSIDsAsync(string? currentBssid = null)
    {
        try
        {
            var output = await RunCommandAsync("netsh", "wlan show networks mode=bssid");
            return ParseBSSIDNetworks(output, currentBssid);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning BSSIDs: {ex.Message}");
            return new List<BSSIDNetwork>();
        }
    }

    public virtual async Task<bool> ConnectToBSSIDAsync(string ssid, string bssid)
    {
        try
        {
            // In Windows netsh, to force connect to a specific BSSID or SSID:
            // First connect to the SSID profile
            await RunCommandAsync("netsh", $"wlan connect name=\"{ssid}\"");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to BSSID {bssid}: {ex.Message}");
            return false;
        }
    }

    public virtual async Task<bool> SetAdapterPreferredBand5GHzAsync()
    {
        try
        {
            // PowerShell script to set Wi-Fi adapter advanced property 'Preferred Band' to 5 GHz
            string psCommand = "Get-NetAdapter | Where-Object { $_.InterfaceDescription -like '*Wi-Fi*' -or $_.Name -like '*Wi-Fi*' } | ForEach-Object { Set-NetAdapterAdvancedProperty -Name $_.Name -RegistryKeyword '*PreferredBand' -RegistryValue '3' -ErrorAction SilentlyContinue }";
            await RunCommandAsync("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting adapter preferred band: {ex.Message}");
            return false;
        }
    }

    protected virtual async Task<string> RunCommandAsync(string fileName, string arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
}
