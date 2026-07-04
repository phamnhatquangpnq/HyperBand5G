// Standardized to production level
// Purpose: Strict C# data models and DTOs for Wi-Fi management, BSSIDs, bands, and auto-switch events
// Dependencies: System

namespace WifiBandLockPro.Core.Models;

public enum WiFiBand
{
    Unknown,
    Band24GHz,
    Band5GHz,
    Band6GHz
}

public enum AutoSwitchMode
{
    Disabled,
    Smart,
    Strict
}

public record AutoSwitchConfig(
    bool Enabled = true,
    AutoSwitchMode Mode = AutoSwitchMode.Smart,
    int PollIntervalMs = 3000,
    int Min5GhzQualityThreshold = 35,
    bool NotifyOnSwitch = true,
    bool Prefer5GhzAdapterProperty = true
);

public record BSSIDNetwork(
    string Ssid,
    string Bssid,
    int SignalQuality,
    int RssiDbm,
    WiFiBand Band,
    int Channel,
    string RadioType,
    string Authentication,
    string Encryption,
    int Score,
    bool IsCurrentConnection
)
{
    public string BandDisplay => Band == WiFiBand.Band24GHz ? "2.4 GHz" : Band == WiFiBand.Band5GHz ? "5 GHz" : Band == WiFiBand.Band6GHz ? "6 GHz" : "Unknown";
}

public record WiFiInterfaceStatus(
    string Name,
    string Description,
    string Guid,
    string MacAddress,
    string State,
    string Ssid,
    string Bssid,
    string NetworkType,
    string RadioType,
    int Channel,
    int ReceiveRateMbps,
    int TransmitRateMbps,
    int SignalQuality,
    int RssiDbm,
    WiFiBand Band
)
{
    public string BandDisplay => Band == WiFiBand.Band24GHz ? "2.4 GHz" : Band == WiFiBand.Band5GHz ? "5 GHz" : Band == WiFiBand.Band6GHz ? "6 GHz" : "Unknown";
}

public record SwitchEventLog(
    string Id,
    DateTime Timestamp,
    string Ssid,
    string FromBssid,
    WiFiBand FromBand,
    string ToBssid,
    WiFiBand ToBand,
    string Reason,
    bool Success
);
