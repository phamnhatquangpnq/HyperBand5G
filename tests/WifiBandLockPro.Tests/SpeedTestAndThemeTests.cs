// Standardized to production level
// Purpose: TDD Unit Tests for Speed Test calculations, Theme color mappings, and Rebranding
// Dependencies: xUnit, WifiBandLockPro.Core

namespace WifiBandLockPro.Tests;

using System;
using Xunit;
using WifiBandLockPro.Core.Models;
using WifiBandLockPro.Core.Services;

public class SpeedTestAndThemeTests
{
    [Fact]
    public void SpeedTestService_CalculateMbps_ShouldCalculateAccurateBandwidth()
    {
        // 15,000,000 bytes in 2,000 ms (2 seconds)
        // 15,000,000 * 8 bits = 120,000,000 bits / 2 seconds = 60 Mbps
        double mbps = SpeedTestService.CalculateMbps(15_000_000, 2000);
        Assert.Equal(60.0, mbps, 1); // 1 decimal place tolerance

        // 50,000,000 bytes in 4,000 ms = 100 Mbps
        double mbps2 = SpeedTestService.CalculateMbps(50_000_000, 4000);
        Assert.Equal(100.0, mbps2, 1);
    }

    [Fact]
    public void ThemeService_GetThemeColors_ShouldReturnCorrectHexCodesForThemes()
    {
        var dark = ThemeService.GetThemeColors("HyperDark");
        Assert.Equal("#0A0D14", dark.Background);
        Assert.Equal("#00D2FF", dark.Accent);

        var cyber = ThemeService.GetThemeColors("CyberNeon");
        Assert.Equal("#0B0813", cyber.Background);
        Assert.Equal("#FF007F", cyber.Accent);

        var oled = ThemeService.GetThemeColors("OLEDBlack");
        Assert.Equal("#000000", oled.Background);
        Assert.Equal("#38BDF8", oled.Accent);

        // New Themes for v2.1
        var matrix = ThemeService.GetThemeColors("MatrixEmerald");
        Assert.Equal("#05130E", matrix.Background);
        Assert.Equal("#00FF66", matrix.Accent);

        var sunset = ThemeService.GetThemeColors("SunsetPulse");
        Assert.Equal("#11081A", sunset.Background);
        Assert.Equal("#FF5E36", sunset.Accent);

        var nordic = ThemeService.GetThemeColors("NordicFrost");
        Assert.Equal("#0F172A", nordic.Background);
        Assert.Equal("#38BDF8", nordic.Accent);

        var gold = ThemeService.GetThemeColors("GoldPrestige");
        Assert.Equal("#0A0A0C", gold.Background);
        Assert.Equal("#F59E0B", gold.Accent);
    }

    [Fact]
    public void LocalizationService_ShouldUseNewRebrandedName()
    {
        var locVn = new LocalizationService("vn");
        Assert.Contains("HYPERBOOST 5G", locVn.AppTitle, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("🚀 ĐO TỐC ĐỘ WI-FI", locVn.SpeedTestTabTitle);

        var locEn = new LocalizationService("en");
        Assert.Contains("HYPERBOOST 5G", locEn.AppTitle, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("🚀 WI-FI SPEED TEST", locEn.SpeedTestTabTitle);
    }
}
