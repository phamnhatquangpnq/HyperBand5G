// Standardized to production level
// Purpose: Immutable records for application configuration, theme, language, and auto-start
// Dependencies: System

namespace HyperBoost.Core.Models;

public record AppSettings(
    bool RunOnStartup = false,
    string Language = "vn",
    string Theme = "KillerDark",
    int PollIntervalSeconds = 3,
    int Min5GhzQualityThreshold = 15,
    bool Prefer5GhzAdapterProperty = true,
    bool MinimizeToTrayOnClose = true,
    bool AutoCleanRamEnabled = true,
    int AutoCleanRamIntervalSeconds = 60
);
