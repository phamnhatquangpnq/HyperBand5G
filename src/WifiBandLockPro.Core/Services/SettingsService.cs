// Standardized to production level
// Purpose: Settings service saving JSON to local AppData and configuring Windows Registry Run key for startup
// Dependencies: System, System.IO, System.Text.Json, Microsoft.Win32

namespace WifiBandLockPro.Core.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using WifiBandLockPro.Core.Models;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private const string RegistryRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WifiBandLockPro";

    public SettingsService(string? customPath = null)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            _settingsFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "WifiBandLockPro");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _settingsFilePath = Path.Combine(dir, "settings.json");
        }
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception)
        {
            // Fallback to default if corrupted
        }
        return new AppSettings();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            string dir = Path.GetDirectoryName(_settingsFilePath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);

            ApplyWindowsStartup(settings.RunOnStartup);
        }
        catch (Exception)
        {
            // Ignore I/O errors in sandbox/restricted environments
        }
    }

    public void ApplyWindowsStartup(bool enable)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
            if (key == null) return;

            if (enable)
            {
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                if (!string.IsNullOrEmpty(exePath) && exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    key.SetValue(AppName, $"\"{exePath}\" --minimized");
                }
            }
            else
            {
                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception)
        {
            // Registry access might fail in unit test sandboxes or non-admin mode
        }
    }
}
