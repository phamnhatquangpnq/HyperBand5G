// Standardized to production level
// Purpose: Record representing system physical memory statistics (Total, Used, Free in MB/GB and percentage)
// Dependencies: System

namespace HyperBoost.Core.Models;

public record SystemMemoryStatus(
    ulong TotalPhysBytes,
    ulong UsedPhysBytes,
    ulong FreePhysBytes,
    int UsedPercentage)
{
    public double TotalPhysGb => TotalPhysBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedPhysGb => UsedPhysBytes / (1024.0 * 1024.0 * 1024.0);
    public double FreePhysGb => FreePhysBytes / (1024.0 * 1024.0 * 1024.0);
    public string DisplayText => $"{UsedPhysGb:F1} GB / {TotalPhysGb:F1} GB ({UsedPercentage}%)";
}
