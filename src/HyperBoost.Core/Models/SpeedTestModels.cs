// Standardized to production level
// Purpose: Immutable records for Wi-Fi Speed Test results (Ping, Jitter, Download, Upload)
// Dependencies: System

namespace HyperBoost.Core.Models;

public record SpeedTestStatus(
    bool IsRunning,
    string CurrentStage,
    int PingMs,
    int JitterMs,
    double DownloadMbps,
    double UploadMbps,
    double ProgressPercentage,
    double CloudDownloadMbps = 0,
    double CloudUploadMbps = 0
);
