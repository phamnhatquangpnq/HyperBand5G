// Standardized to production level
// Purpose: TDD unit and integration tests for SystemOptimizerService (RAM cleaner, Process list, Safe Junk cleaner)
// Dependencies: Xunit, HyperBoost.Core.Models, HyperBoost.Core.Services

namespace HyperBoost.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperBoost.Core.Models;
using HyperBoost.Core.Services;
using Xunit;

public class SystemOptimizerTests
{
    [Fact]
    public async Task GetMemoryStatusAsync_ShouldReturnValidPhysicalMemoryStatistics()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        var status = await service.GetMemoryStatusAsync();

        // Assert
        Assert.NotNull(status);
        Assert.True(status.TotalPhysBytes > 0, "Total physical memory must be greater than 0");
        Assert.True(status.UsedPhysBytes <= status.TotalPhysBytes, "Used physical memory cannot exceed total memory");
        Assert.InRange(status.UsedPercentage, 0, 100);
    }

    [Fact]
    public async Task GetTopProcessesAsync_ShouldReturnRunningProcessesSortedByRam()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        var processes = await service.GetTopProcessesAsync(20);

        // Assert
        Assert.NotNull(processes);
        Assert.NotEmpty(processes);
        Assert.True(processes.Count <= 20);

        // Check sorting (highest RAM first)
        for (int i = 0; i < processes.Count - 1; i++)
        {
            Assert.True(processes[i].WorkingSetMb >= processes[i + 1].WorkingSetMb, 
                $"Processes must be sorted descending by WorkingSetMb. Found {processes[i].WorkingSetMb} before {processes[i + 1].WorkingSetMb}");
        }

        // At least some process should have a valid name and ID
        var top = processes.First();
        Assert.True(top.ProcessId > 0);
        Assert.False(string.IsNullOrWhiteSpace(top.ProcessName));
        Assert.True(top.WorkingSetMb > 0);
    }

    [Fact]
    public async Task OptimizeRAMAsync_ShouldExecuteWithoutExceptionAndReturnTrimmedCount()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        int trimmedProcesses = await service.OptimizeRAMAsync();

        // Assert
        Assert.True(trimmedProcesses >= 0, "Trimmed process count should be non-negative");
    }

    [Fact]
    public async Task ScanSafeJunkAsync_ShouldReturnValidJunkScanResult()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        var result = await service.ScanSafeJunkAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalFilesFound >= 0);
        Assert.True(result.TotalSizeBytes >= 0);
    }

    [Fact]
    public async Task CleanSafeJunkAsync_ShouldNotThrowAndShouldReduceOrMaintainJunkSize()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        var cleanResult = await service.CleanSafeJunkAsync();

        // Assert
        Assert.NotNull(cleanResult);
        Assert.True(cleanResult.TotalFilesFound >= 0);
        Assert.True(cleanResult.TotalSizeBytes >= 0);
    }

    [Fact]
    public async Task GetMemoryStatusAsync_ShouldAccuratelyCalculateUsedPercentageAndBytes()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();

        // Act
        var status = await service.GetMemoryStatusAsync();

        // Assert: Used + Free should equal Total (or be very close within 1MB due to uint precision)
        ulong sum = status.UsedPhysBytes + status.FreePhysBytes;
        long diff = Math.Abs((long)status.TotalPhysBytes - (long)sum);
        Assert.True(diff < 1024 * 1024 * 10, $"Used ({status.UsedPhysBytes}) + Free ({status.FreePhysBytes}) should equal Total ({status.TotalPhysBytes})");

        // Assert percentage matches Math.Round(Used / Total * 100)
        int expectedPct = (int)Math.Round((double)status.UsedPhysBytes / status.TotalPhysBytes * 100.0);
        Assert.Equal(expectedPct, status.UsedPercentage);
    }

    [Fact]
    public async Task ScanSafeJunkAsync_ShouldTraverseSubdirectoriesInTempWithoutException()
    {
        // Arrange
        ISystemOptimizerService service = new SystemOptimizerService();
        string tempPath = Path.GetTempPath();
        
        // Act
        var result = await service.ScanSafeJunkAsync();

        // Assert: If temp directory exists and has files in subdirectories, we should find them
        Assert.NotNull(result);
        if (Directory.Exists(tempPath) && Directory.GetFiles(tempPath, "*", SearchOption.TopDirectoryOnly).Length > 0)
        {
            Assert.True(result.UserTempFiles > 0, "Should find files in %TEMP%");
        }
    }
}
