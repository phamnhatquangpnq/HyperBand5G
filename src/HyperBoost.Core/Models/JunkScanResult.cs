// Standardized to production level
// Purpose: Record representing results of a safe system junk file scan or cleanup operation
// Dependencies: System

namespace HyperBoost.Core.Models;

public record JunkScanResult(
    int TotalFilesFound,
    long TotalSizeBytes,
    int UserTempFiles,
    long UserTempBytes,
    int WinTempFiles,
    long WinTempBytes,
    int RecycleBinItems,
    long RecycleBinBytes,
    int LogFiles,
    long LogBytes,
    long FreedBytes = 0)
{
    public double TotalSizeMb => TotalSizeBytes / (1024.0 * 1024.0);
    public string TotalSizeDisplay => TotalSizeMb >= 1024 ? $"{TotalSizeMb / 1024.0:F2} GB" : $"{TotalSizeMb:F1} MB";
    public string FreedDisplay => FreedBytes >= 1_073_741_824 ? $"{(double)FreedBytes / 1_073_741_824:F2} GB" :
                                  FreedBytes >= 1_048_576 ? $"{(double)FreedBytes / 1_048_576:F2} MB" :
                                  FreedBytes > 0 ? $"{(double)FreedBytes / 1024:F2} KB" : "0 MB";
}
