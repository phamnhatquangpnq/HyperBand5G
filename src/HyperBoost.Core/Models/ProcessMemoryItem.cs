// Standardized to production level
// Purpose: Record representing a running system process with memory consumption and executable path for icon extraction
// Dependencies: System

namespace HyperBoost.Core.Models;

public record ProcessMemoryItem(
    int ProcessId,
    string ProcessName,
    string DisplayName,
    string? ExecutablePath,
    double WorkingSetMb,
    bool CanEndTask)
{
    public string WorkingSetDisplay => $"{WorkingSetMb:F1} MB";
}
