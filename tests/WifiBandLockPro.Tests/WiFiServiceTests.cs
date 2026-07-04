// Standardized to production level
// Purpose: TDD Unit Tests for WiFiService (Windows netsh command parser and band calculation)
// Dependencies: xUnit, WifiBandLockPro.Core

namespace WifiBandLockPro.Tests;

using System.Linq;
using Xunit;
using WifiBandLockPro.Core.Models;
using WifiBandLockPro.Core.Services;

public class WiFiServiceTests
{
    private const string MockNetshInterfacesOutput = @"
There is 1 interface on the system:

    Name                   : Wi-Fi
    Description            : Intel(R) Wi-Fi 6 AX201 160MHz
    GUID                   : 12345678-1234-1234-1234-123456789012
    Physical address       : 00:11:22:33:44:55
    State                  : connected
    SSID                   : Not Wifi_2.4
    BSSID                  : 24:0b:2a:3f:2a:99
    Network type           : Infrastructure
    Radio type             : 802.11n
    Authentication         : WPA2-Personal
    Cipher                 : CCMP
    Connection mode        : Auto Connect
    Channel                : 6
    Receive rate (Mbps)    : 144
    Transmit rate (Mbps)   : 144
    Signal                 : 81%
    Profile                : Not Wifi_2.4
";

    private const string MockNetshNetworksBssidOutput = @"
Interface name : Wi-Fi
There are 2 networks currently visible.

SSID 1 : Not Wifi_2.4
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 24:0b:2a:3f:2a:9a
         Signal             : 80%
         Radio type         : 802.11ac
         Channel            : 149
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54
    BSSID 2                 : 24:0b:2a:3f:2a:99
         Signal             : 70%
         Radio type         : 802.11n
         Channel            : 6
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 2 : Hong Bich 5GHz
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 4c:12:e8:db:9c:81
         Signal             : 47%
         Radio type         : 802.11ac
         Channel            : 36
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54
";

    [Fact]
    public void GetBandFromChannel_ShouldReturnCorrectBand()
    {
        var service = new WiFiService();
        
        Assert.Equal(WiFiBand.Band24GHz, service.GetBandFromChannel(1));
        Assert.Equal(WiFiBand.Band24GHz, service.GetBandFromChannel(6));
        Assert.Equal(WiFiBand.Band24GHz, service.GetBandFromChannel(11));
        Assert.Equal(WiFiBand.Band24GHz, service.GetBandFromChannel(14));
        Assert.Equal(WiFiBand.Band5GHz, service.GetBandFromChannel(36));
        Assert.Equal(WiFiBand.Band5GHz, service.GetBandFromChannel(149));
        Assert.Equal(WiFiBand.Band5GHz, service.GetBandFromChannel(161));
        Assert.Equal(WiFiBand.Band6GHz, service.GetBandFromChannel(193));
    }

    [Fact]
    public void ParseInterfaceStatus_ShouldParseCorrectly_AndIdentify24GHz()
    {
        var service = new WiFiService();
        var status = service.ParseInterfaceStatus(MockNetshInterfacesOutput);

        Assert.NotNull(status);
        Assert.Equal("Wi-Fi", status.Name);
        Assert.Equal("Not Wifi_2.4", status.Ssid);
        Assert.Equal("24:0b:2a:3f:2a:99", status.Bssid);
        Assert.Equal(6, status.Channel);
        Assert.Equal(81, status.SignalQuality);
        Assert.Equal("802.11n", status.RadioType);
        Assert.Equal(WiFiBand.Band24GHz, status.Band); // Channel 6 -> 2.4GHz
        Assert.Equal("connected", status.State);
    }

    [Fact]
    public void ParseBSSIDNetworks_ShouldParseAndComputeScoresAndBands()
    {
        var service = new WiFiService();
        var networks = service.ParseBSSIDNetworks(MockNetshNetworksBssidOutput, "24:0b:2a:3f:2a:99");

        Assert.Equal(3, networks.Count);

        // Check BSSID 1 of Not Wifi_2.4 (5GHz)
        var bssid5g = networks.FirstOrDefault(n => n.Bssid == "24:0b:2a:3f:2a:9a");
        Assert.NotNull(bssid5g);
        Assert.Equal("Not Wifi_2.4", bssid5g.Ssid);
        Assert.Equal(WiFiBand.Band5GHz, bssid5g.Band);
        Assert.Equal(149, bssid5g.Channel);
        Assert.Equal(80, bssid5g.SignalQuality);
        Assert.False(bssid5g.IsCurrentConnection);
        Assert.True(bssid5g.Score > 70); // 5GHz score boost

        // Check BSSID 2 of Not Wifi_2.4 (2.4GHz - currently connected)
        var bssid24g = networks.FirstOrDefault(n => n.Bssid == "24:0b:2a:3f:2a:99");
        Assert.NotNull(bssid24g);
        Assert.Equal(WiFiBand.Band24GHz, bssid24g.Band);
        Assert.True(bssid24g.IsCurrentConnection);

        // Check Hong Bich 5GHz
        var hongBich = networks.FirstOrDefault(n => n.Ssid == "Hong Bich 5GHz");
        Assert.NotNull(hongBich);
        Assert.Equal(WiFiBand.Band5GHz, hongBich.Band);
        Assert.Equal(47, hongBich.SignalQuality);
    }
}
