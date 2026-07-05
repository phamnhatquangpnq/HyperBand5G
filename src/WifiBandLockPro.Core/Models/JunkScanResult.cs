// Standardized to production level
// Purpose: Record representing results of a safe system junk file scan or cleanup operation
// Dependencies: System

namespace WifiBandLockPro.Core.Models;

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
    long LogBytes)
{
    public double TotalSizeMb => TotalSizeBytes / (1024.0 * 1024.0);
    public string TotalSizeDisplay => TotalSizeMb >= 1024 ? $"{TotalSizeMb / 1024.0:F2} GB" : $"{TotalSizeMb:F1} MB";
}
