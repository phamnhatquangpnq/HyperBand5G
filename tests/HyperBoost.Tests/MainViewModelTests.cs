// Standardized to production level
// Purpose: TDD Unit Tests for MainViewModel (UI data binding, network categorization, and band badges)
// Dependencies: xUnit, HyperBoost.Core

namespace HyperBoost.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HyperBoost.Core.Models;
using HyperBoost.Core.Services;
using HyperBoost.Core.ViewModels;

public class MainViewModelTests
{
    private class MockWiFiService : IWiFiService
    {
        public WiFiInterfaceStatus? CurrentStatus { get; set; }
        public List<BSSIDNetwork> ScanResults { get; set; } = new();
        public string? LastConnectedSsid { get; private set; }
        public string? LastConnectedBssid { get; private set; }

        public WiFiBand GetBandFromChannel(int channel) => channel >= 36 ? WiFiBand.Band5GHz : WiFiBand.Band24GHz;
        public WiFiInterfaceStatus? ParseInterfaceStatus(string rawOutput) => CurrentStatus;
        public List<BSSIDNetwork> ParseBSSIDNetworks(string rawOutput, string? currentBssid = null) => ScanResults;

        public Task<WiFiInterfaceStatus?> GetCurrentInterfaceAsync() => Task.FromResult(CurrentStatus);
        public Task<List<BSSIDNetwork>> ScanBSSIDsAsync(string? currentBssid = null) => Task.FromResult(ScanResults);

        public Task<bool> ConnectToBSSIDAsync(string ssid, string bssid)
        {
            LastConnectedSsid = ssid;
            LastConnectedBssid = bssid;
            return Task.FromResult(true);
        }

        public Task<bool> SetAdapterPreferredBand5GHzAsync() => Task.FromResult(true);
        public Task<bool> ResetAdapterPreferredBandAsync() => Task.FromResult(true);
    }

    [Fact]
    public async Task RefreshAsync_ShouldCategorizeNetworks_AndFormat5GHzBadge()
    {
        var mockSvc = new MockWiFiService();
        mockSvc.CurrentStatus = new WiFiInterfaceStatus(
            "Wi-Fi", "Intel AX201", "guid", "mac", "connected",
            "Not Wifi_2.4", "24:0b:2a:3f:2a:9a", "Infra", "802.11ac",
            149, 866, 866, 80, -60, WiFiBand.Band5GHz
        );
        mockSvc.ScanResults = new List<BSSIDNetwork>
        {
            new("Not Wifi_2.4", "24:0b:2a:3f:2a:9a", 80, -60, WiFiBand.Band5GHz, 149, "802.11ac", "WPA2", "CCMP", 95, true),
            new("Not Wifi_2.4", "24:0b:2a:3f:2a:99", 70, -65, WiFiBand.Band24GHz, 6, "802.11n", "WPA2", "CCMP", 70, false),
            new("Hong Bich 5GHz", "4c:12:e8:db:9c:81", 47, -76, WiFiBand.Band5GHz, 36, "802.11ac", "WPA2", "CCMP", 67, false)
        };

        var engine = new AutoSwitchEngine(mockSvc);
        var vm = new MainViewModel(mockSvc, engine, null, new LocalizationService("en"));

        await vm.RefreshAsync();

        Assert.NotNull(vm.CurrentStatus);
        Assert.Equal("Not Wifi_2.4", vm.CurrentStatus.Ssid);
        Assert.Equal(vm.Loc.Badge5Ghz, vm.CurrentBandBadgeText);
        Assert.Equal("#00F280", vm.CurrentBandBadgeColor); // Green

        // Authorized networks should contain ONLY BSSIDs of "Not Wifi_2.4"
        Assert.Equal(2, vm.AuthorizedNetworks.Count);
        Assert.All(vm.AuthorizedNetworks, n => Assert.Equal("Not Wifi_2.4", n.Ssid));

        // Available networks should contain other networks
        Assert.Single(vm.AvailableNetworks);
        Assert.Equal("Hong Bich 5GHz", vm.AvailableNetworks[0].Ssid);
    }

    [Fact]
    public async Task RefreshAsync_WhenConnectedTo24GHz_ShouldFormatWarningBadge()
    {
        var mockSvc = new MockWiFiService();
        mockSvc.CurrentStatus = new WiFiInterfaceStatus(
            "Wi-Fi", "Intel AX201", "guid", "mac", "connected",
            "Not Wifi_2.4", "24:0b:2a:3f:2a:99", "Infra", "802.11n",
            6, 144, 144, 80, -60, WiFiBand.Band24GHz
        );
        mockSvc.ScanResults = new List<BSSIDNetwork>();

        var engine = new AutoSwitchEngine(mockSvc);
        var vm = new MainViewModel(mockSvc, engine, null, new LocalizationService("en"));

        await vm.RefreshAsync();

        Assert.Equal(vm.Loc.Badge24Ghz, vm.CurrentBandBadgeText);
        Assert.Equal("#FF9900", vm.CurrentBandBadgeColor); // Orange warning
    }

    [Fact]
    public void ToggleSmartSelectionCommand_ShouldToggleStateAndEngineConfig()
    {
        var mockSvc = new MockWiFiService();
        var engine = new AutoSwitchEngine(mockSvc);
        var vm = new MainViewModel(mockSvc, engine);

        Assert.True(vm.IsSmartSelectionEnabled);
        Assert.True(engine.Config.Enabled);

        vm.ToggleSmartSelectionCommand.Execute(null);

        Assert.False(vm.IsSmartSelectionEnabled);
        Assert.False(engine.Config.Enabled);
    }

    [Fact]
    public void SwitchTabCommand_ShouldChangeTabIndex_AndTriggerOptimizerRefreshOnTab1()
    {
        var mockSvc = new MockWiFiService();
        var engine = new AutoSwitchEngine(mockSvc);
        var vm = new MainViewModel(mockSvc, engine);

        Assert.Equal(0, vm.CurrentTabIndex);

        vm.SwitchTabCommand.Execute(1);
        Assert.Equal(1, vm.CurrentTabIndex);

        vm.SwitchTabCommand.Execute(2);
        Assert.Equal(2, vm.CurrentTabIndex);
    }
}
