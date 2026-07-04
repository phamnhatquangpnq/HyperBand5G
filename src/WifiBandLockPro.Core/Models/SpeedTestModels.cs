// Standardized to production level
// Purpose: Immutable records for Wi-Fi Speed Test results (Ping, Jitter, Download, Upload)
// Dependencies: System

namespace WifiBandLockPro.Core.Models;

public record SpeedTestStatus(
    bool IsRunning,
    string CurrentStage,
    int PingMs,
    int JitterMs,
    double DownloadMbps,
    double UploadMbps,
    double ProgressPercentage
);
