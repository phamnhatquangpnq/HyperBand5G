// Standardized to production level
// Purpose: TDD Unit & State Machine Tests for AutoSwitchEngine (Band steering from 2.4GHz to 5GHz)
// Dependencies: xUnit, HyperBoost.Core

namespace HyperBoost.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HyperBoost.Core.Models;
using HyperBoost.Core.Services;

public class AutoSwitchEngineTests
{
    private class MockWiFiService : IWiFiService
    {
        public WiFiInterfaceStatus? CurrentStatus { get; set; }
        public List<BSSIDNetwork> ScanResults { get; set; } = new();
        public string? LastConnectedSsid { get; private set; }
        public string? LastConnectedBssid { get; private set; }
        public bool PreferredBandSet { get; private set; } = false;

        public WiFiBand GetBandFromChannel(int channel) => channel >= 36 ? WiFiBand.Band5GHz : WiFiBand.Band24GHz;
        public WiFiInterfaceStatus? ParseInterfaceStatus(string rawOutput) => CurrentStatus;
        public List<BSSIDNetwork> ParseBSSIDNetworks(string rawOutput, string? currentBssid = null) => ScanResults;

        public Task<WiFiInterfaceStatus?> GetCurrentInterfaceAsync() => Task.FromResult(CurrentStatus);
        public Task<List<BSSIDNetwork>> ScanBSSIDsAsync(string? currentBssid = null) => Task.FromResult(ScanResults);

        public Task<bool> ConnectToBSSIDAsync(string ssid, string bssid)
        {
            LastConnectedSsid = ssid;
            LastConnectedBssid = bssid;
            // Simulate switch
            if (CurrentStatus != null)
            {
                CurrentStatus = CurrentStatus with { Bssid = bssid, Band = WiFiBand.Band5GHz, Channel = 149 };
            }
            return Task.FromResult(true);
        }

        public Task<bool> SetAdapterPreferredBand5GHzAsync()
        {
            PreferredBandSet = true;
            return Task.FromResult(true);
        }

        public Task<bool> ResetAdapterPreferredBandAsync()
        {
            PreferredBandSet = false;
            return Task.FromResult(true);
        }
    }

    [Fact]
    public async Task EvaluateAndSwitchAsync_WhenConnectedTo24GHz_And5GHzAvailable_ShouldSwitchTo5GHz()
    {
        var mockSvc = new MockWiFiService();
        mockSvc.CurrentStatus = new WiFiInterfaceStatus(
            "Wi-Fi", "Intel AX201", "guid", "mac", "connected",
            "Not Wifi_2.4", "24:0b:2a:3f:2a:99", "Infra", "802.11n",
            6, 144, 144, 80, -60, WiFiBand.Band24GHz
        );
        mockSvc.ScanResults = new List<BSSIDNetwork>
        {
            new("Not Wifi_2.4", "24:0b:2a:3f:2a:99", 80, -60, WiFiBand.Band24GHz, 6, "802.11n", "WPA2", "CCMP", 80, true),
            new("Not Wifi_2.4", "24:0b:2a:3f:2a:9a", 75, -62, WiFiBand.Band5GHz, 149, "802.11ac", "WPA2", "CCMP", 95, false)
        };

        var engine = new AutoSwitchEngine(mockSvc);
        SwitchEventLog? firedEvent = null;
        engine.OnSwitchEvent += (s, e) => firedEvent = e;

        var result = await engine.EvaluateAndSwitchAsync();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Not Wifi_2.4", result.Ssid);
        Assert.Equal(WiFiBand.Band24GHz, result.FromBand);
        Assert.Equal(WiFiBand.Band5GHz, result.ToBand);
        Assert.Equal("24:0b:2a:3f:2a:9a", result.ToBssid);
        Assert.Equal("24:0b:2a:3f:2a:9a", mockSvc.LastConnectedBssid);
        Assert.True(mockSvc.PreferredBandSet);
        Assert.NotNull(firedEvent);
        Assert.Single(engine.EventLogs);
    }

    [Fact]
    public async Task EvaluateAndSwitchAsync_WhenConnectedTo5GHz_ShouldNotSwitch()
    {
        var mockSvc = new MockWiFiService();
        mockSvc.CurrentStatus = new WiFiInterfaceStatus(
            "Wi-Fi", "Intel AX201", "guid", "mac", "connected",
            "Not Wifi_2.4", "24:0b:2a:3f:2a:9a", "Infra", "802.11ac",
            149, 866, 866, 80, -60, WiFiBand.Band5GHz
        );

        var engine = new AutoSwitchEngine(mockSvc);
        var result = await engine.EvaluateAndSwitchAsync();

        Assert.Null(result);
        Assert.Null(mockSvc.LastConnectedBssid);
        Assert.Empty(engine.EventLogs);
    }

    [Fact]
    public async Task EvaluateAndSwitchAsync_WhenDisabled_ShouldNotSwitch()
    {
        var mockSvc = new MockWiFiService();
        mockSvc.CurrentStatus = new WiFiInterfaceStatus(
            "Wi-Fi", "Intel AX201", "guid", "mac", "connected",
            "Not Wifi_2.4", "24:0b:2a:3f:2a:99", "Infra", "802.11n",
            6, 144, 144, 80, -60, WiFiBand.Band24GHz
        );

        var engine = new AutoSwitchEngine(mockSvc, new AutoSwitchConfig(false, AutoSwitchMode.Smart, 3000, 35, true, true));
        var result = await engine.EvaluateAndSwitchAsync();

        Assert.Null(result);
        Assert.Null(mockSvc.LastConnectedBssid);
    }
}
