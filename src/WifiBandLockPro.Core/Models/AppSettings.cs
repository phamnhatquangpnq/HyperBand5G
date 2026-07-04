// Standardized to production level
// Purpose: Immutable records for application configuration, theme, language, and auto-start
// Dependencies: System

namespace WifiBandLockPro.Core.Models;

public record AppSettings(
    bool RunOnStartup = false,
    string Language = "vn",
    string Theme = "KillerDark",
    int PollIntervalSeconds = 3,
    int Min5GhzQualityThreshold = 35,
    bool Prefer5GhzAdapterProperty = true,
    bool MinimizeToTrayOnClose = true
);
