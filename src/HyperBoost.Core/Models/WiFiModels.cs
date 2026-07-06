// Standardized to production level
// Purpose: Strict C# data models and DTOs for Wi-Fi management, BSSIDs, bands, and auto-switch events
// Dependencies: System

namespace HyperBoost.Core.Models;

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
    int Min5GhzQualityThreshold = 15,
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
    public string SignalDisplay => $"{SignalQuality}% ({RssiDbm} dBm)";
    public string ScoreDisplay => $"{Score} pts";
    public string PhyKindDisplay => $"{RadioType} / {Channel}";
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
    public string SignalDisplay => $"{SignalQuality}% ({RssiDbm} dBm)";
    public string ScoreDisplay => $"{0} pts";
    public string PhyKindDisplay => $"{RadioType} / {Channel}";
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
    bool Success,
    string Category = "WIFI"
)
{
    public string CategoryBadge => Category switch
    {
        "RAM" => "🚀 RAM Boost",
        "JUNK" => "🧹 Junk Clean",
        "COMPRESS" => "⚡ Compress",
        _ => "📡 Wi-Fi"
    };

    public string BadgeBgColor => Category switch
    {
        "RAM" => "#163326",      // Soft dark mint
        "JUNK" => "#332D1A",     // Soft dark amber
        "COMPRESS" => "#2E1E3B", // Soft dark purple
        _ => "#1A2E40"           // Soft dark blue (WiFi)
    };

    public string BadgeTextColor => Category switch
    {
        "RAM" => "#6EE7B7",      // Pastel Mint
        "JUNK" => "#FDE047",     // Pastel Yellow
        "COMPRESS" => "#C084FC", // Pastel Purple
        _ => "#38BDF8"           // Pastel Sky Blue (WiFi)
    };

    public string MessageTextColor => Category switch
    {
        "RAM" => "#E6FFFA",      // Light soft teal/green
        "JUNK" => "#FEFCE8",     // Light soft yellow
        "COMPRESS" => "#FAF5FF", // Light soft lavender
        _ => "#E0F2FE"           // Light soft blue
    };
}
