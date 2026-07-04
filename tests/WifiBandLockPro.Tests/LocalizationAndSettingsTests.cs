// Standardized to production level
// Purpose: TDD Unit Tests for Localization, Settings persistence, and Band display formatting
// Dependencies: xUnit, WifiBandLockPro.Core

namespace WifiBandLockPro.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using WifiBandLockPro.Core.Models;
using WifiBandLockPro.Core.Services;

public class LocalizationAndSettingsTests : IDisposable
{
    private readonly string _testSettingsPath;

    public LocalizationAndSettingsTests()
    {
        _testSettingsPath = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_testSettingsPath))
        {
            File.Delete(_testSettingsPath);
        }
    }

    [Fact]
    public void BSSIDNetwork_BandDisplay_ShouldFormatCorrectly()
    {
        var net24 = new BSSIDNetwork("Test", "00:11:22:33:44:55", 80, -60, WiFiBand.Band24GHz, 6, "802.11n", "WPA2", "CCMP", 70, false);
        var net50 = new BSSIDNetwork("Test", "00:11:22:33:44:66", 90, -50, WiFiBand.Band5GHz, 149, "802.11ac", "WPA2", "CCMP", 95, true);
        var net60 = new BSSIDNetwork("Test", "00:11:22:33:44:77", 85, -55, WiFiBand.Band6GHz, 193, "802.11ax", "WPA3", "CCMP", 90, false);

        Assert.Equal("2.4 GHz", net24.BandDisplay);
        Assert.Equal("5 GHz", net50.BandDisplay);
        Assert.Equal("6 GHz", net60.BandDisplay);
    }

    [Fact]
    public void LocalizationService_ShouldSwitchBetweenVietnameseAndEnglish()
    {
        var loc = new LocalizationService("vn");
        Assert.True(loc.IsVietnamese);
        Assert.Equal("HYPERBAND 5G SUITE", loc.AppSubtitle);

        loc.SetLanguage("en");
        Assert.False(loc.IsVietnamese);
        Assert.Equal("HYPERBAND 5G SUITE", loc.AppSubtitle);
    }

    [Fact]
    public async Task SettingsService_ShouldSaveAndLoadSettingsCorrectly()
    {
        var svc = new SettingsService(_testSettingsPath);
        var settings = new AppSettings(
            RunOnStartup: true,
            Language: "en",
            Theme: "CyberNeon",
            PollIntervalSeconds: 5,
            Min5GhzQualityThreshold: 40
        );

        await svc.SaveSettingsAsync(settings);
        Assert.True(File.Exists(_testSettingsPath));

        var loaded = await svc.LoadSettingsAsync();
        Assert.True(loaded.RunOnStartup);
        Assert.Equal("en", loaded.Language);
        Assert.Equal("CyberNeon", loaded.Theme);
        Assert.Equal(5, loaded.PollIntervalSeconds);
        Assert.Equal(40, loaded.Min5GhzQualityThreshold);
    }
}
