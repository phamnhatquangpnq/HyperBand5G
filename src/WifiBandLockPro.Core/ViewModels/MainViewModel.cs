// Standardized to production level
// Purpose: Main ViewModel coordinating UI state, network tables, speed test, RAM optimizer, junk cleaner, localization, and theme switching
// Dependencies: System.Collections.ObjectModel, System.Linq, WifiBandLockPro.Core.Models, WifiBandLockPro.Core.Services

namespace WifiBandLockPro.Core.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WifiBandLockPro.Core.Models;
using WifiBandLockPro.Core.Services;

public class MainViewModel : ViewModelBase
{
    private readonly IWiFiService _wifiService;
    private readonly AutoSwitchEngine _autoSwitchEngine;
    private readonly ISettingsService _settingsService;
    public ISpeedTestService SpeedTestService { get; }
    public ISystemOptimizerService SystemOptimizerService { get; }
    public LocalizationService Loc { get; }

    public event EventHandler<string>? OnThemeChangedRequest;

    private bool _isSmartSelectionEnabled;
    private int _currentTabIndex; // 0 = Dashboard, 1 = Speed Test, 2 = Optimizer, 3 = Settings
    private WiFiInterfaceStatus? _currentStatus;
    private string _currentBandBadgeText = "Scanning...";
    private string _currentBandBadgeColor = "#64748B"; // Muted gray
    private AppSettings _currentSettings = new();
    private SpeedTestStatus _speedTestStatus = new(false, "Ready / Sẵn sàng", 0, 0, 0, 0, 0);
    
    private SystemMemoryStatus _memoryStatus = new(16UL * 1024 * 1024 * 1024, 8UL * 1024 * 1024 * 1024, 8UL * 1024 * 1024 * 1024, 50);
    private JunkScanResult _junkResult = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private bool _isScanningJunk;

    public ObservableCollection<BSSIDNetwork> AuthorizedNetworks { get; } = new();
    public ObservableCollection<BSSIDNetwork> AvailableNetworks { get; } = new();
    public ObservableCollection<SwitchEventLog> ActivityLogs { get; } = new();
    public ObservableCollection<ProcessMemoryItem> TopProcesses { get; } = new();

    public ICommand ToggleSmartSelectionCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ForceSwitch5GhzCommand { get; }
    public ICommand ConnectToBssidCommand { get; }
    public ICommand SwitchTabCommand { get; }
    public ICommand SwitchLanguageCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand StartSpeedTestCommand { get; }
    public ICommand ClearLogsCommand { get; }
    public ICommand RefreshOptimizerCommand { get; }
    public ICommand EndTaskCommand { get; }
    public ICommand OptimizeRamCommand { get; }
    public ICommand ScanJunkCommand { get; }
    public ICommand CleanJunkCommand { get; }
    public ICommand UltraCompressCommand { get; }

    public bool IsSmartSelectionEnabled
    {
        get => _isSmartSelectionEnabled;
        set
        {
            if (SetProperty(ref _isSmartSelectionEnabled, value))
            {
                _autoSwitchEngine.Config = _autoSwitchEngine.Config with { Enabled = value };
            }
        }
    }

    public int CurrentTabIndex
    {
        get => _currentTabIndex;
        set
        {
            if (SetProperty(ref _currentTabIndex, value))
            {
                if (value == 2)
                {
                    _ = RefreshOptimizerAsync();
                }
            }
        }
    }

    public WiFiInterfaceStatus? CurrentStatus
    {
        get => _currentStatus;
        set => SetProperty(ref _currentStatus, value);
    }

    public string CurrentBandBadgeText
    {
        get => _currentBandBadgeText;
        set => SetProperty(ref _currentBandBadgeText, value);
    }

    public string CurrentBandBadgeColor
    {
        get => _currentBandBadgeColor;
        set => SetProperty(ref _currentBandBadgeColor, value);
    }

    public AppSettings CurrentSettings
    {
        get => _currentSettings;
        set
        {
            if (SetProperty(ref _currentSettings, value))
            {
                _autoSwitchEngine.Config = _autoSwitchEngine.Config with
                {
                    PollIntervalMs = value.PollIntervalSeconds * 1000,
                    Min5GhzQualityThreshold = value.Min5GhzQualityThreshold
                };
                OnThemeChangedRequest?.Invoke(this, value.Theme);
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }
    }

    public string SelectedTheme
    {
        get => CurrentSettings.Theme;
        set
        {
            if (!string.Equals(CurrentSettings.Theme, value, StringComparison.OrdinalIgnoreCase))
            {
                CurrentSettings = CurrentSettings with { Theme = value };
                OnPropertyChanged();
                OnThemeChangedRequest?.Invoke(this, value);
                _ = _settingsService.SaveSettingsAsync(CurrentSettings);
            }
        }
    }

    public SpeedTestStatus SpeedTestStatus
    {
        get => _speedTestStatus;
        set => SetProperty(ref _speedTestStatus, value);
    }

    public SystemMemoryStatus MemoryStatus
    {
        get => _memoryStatus;
        set => SetProperty(ref _memoryStatus, value);
    }

    public JunkScanResult JunkResult
    {
        get => _junkResult;
        set => SetProperty(ref _junkResult, value);
    }

    public bool IsScanningJunk
    {
        get => _isScanningJunk;
        set => SetProperty(ref _isScanningJunk, value);
    }

    public MainViewModel(IWiFiService wifiService, AutoSwitchEngine autoSwitchEngine, ISettingsService? settingsService = null, LocalizationService? locService = null, ISpeedTestService? speedTestService = null, ISystemOptimizerService? optimizerService = null)
    {
        _wifiService = wifiService ?? throw new ArgumentNullException(nameof(wifiService));
        _autoSwitchEngine = autoSwitchEngine ?? throw new ArgumentNullException(nameof(autoSwitchEngine));
        _settingsService = settingsService ?? new SettingsService();
        Loc = locService ?? new LocalizationService("vn");
        SpeedTestService = speedTestService ?? new SpeedTestService();
        SystemOptimizerService = optimizerService ?? new SystemOptimizerService();

        _isSmartSelectionEnabled = _autoSwitchEngine.Config.Enabled;

        ToggleSmartSelectionCommand = new RelayCommand(_ => {
            IsSmartSelectionEnabled = !IsSmartSelectionEnabled;
            ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "Wi-Fi Steering", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, $"🔄 [SMART STEERING] Chế độ tự động khóa & điều hướng 5GHz: {(IsSmartSelectionEnabled ? "BẬT (ACTIVE)" : "TẮT")}", true, "WIFI"));
        });
        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        ForceSwitch5GhzCommand = new RelayCommand(async _ => await ForceSwitchTo5GhzAsync());
        ConnectToBssidCommand = new RelayCommand(async param =>
        {
            if (param is BSSIDNetwork target && CurrentStatus != null)
            {
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, target.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, target.Bssid, target.Band, $"🚀 [MANUAL SWITCH] Đang kết nối tới điểm truy cập BSSID {target.Bssid} ({target.BandDisplay} - {target.SignalQuality}%)...", true, "WIFI"));
                await _wifiService.ConnectToBSSIDAsync(target.Ssid, target.Bssid);
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, target.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, target.Bssid, target.Band, $"✅ [MANUAL SWITCH] Đã khóa & kết nối thành công BSSID {target.Bssid} ({target.BandDisplay})!", true, "WIFI"));
                await RefreshAsync();
            }
        });
        ClearLogsCommand = new RelayCommand(_ => ActivityLogs.Clear());
        
        SwitchTabCommand = new RelayCommand(param =>
        {
            if (int.TryParse(param?.ToString(), out int idx))
            {
                CurrentTabIndex = idx;
                if (CurrentTabIndex == 1)
                {
                    _ = RefreshOptimizerAsync();
                }
            }
        });
        
        SwitchLanguageCommand = new RelayCommand(async param =>
        {
            string lang = param as string ?? "vn";
            Loc.SetLanguage(lang);
            OnPropertyChanged(nameof(Loc));
            CurrentSettings = CurrentSettings with { Language = lang };
            await _settingsService.SaveSettingsAsync(CurrentSettings);
            await RefreshAsync();
        });

        SaveSettingsCommand = new RelayCommand(async _ =>
        {
            await _settingsService.SaveSettingsAsync(CurrentSettings);
            OnThemeChangedRequest?.Invoke(this, CurrentSettings.Theme);
            CurrentTabIndex = 0; // Return to dashboard
        });

        StartSpeedTestCommand = new RelayCommand(async _ =>
        {
            if (SpeedTestStatus.IsRunning) return;
            SpeedTestStatus = await SpeedTestService.RunSpeedTestAsync();
        }, _ => !SpeedTestStatus.IsRunning);

        RefreshOptimizerCommand = new RelayCommand(async _ => await RefreshOptimizerAsync());
        
        EndTaskCommand = new RelayCommand(async param =>
        {
            if (param is int pid || (param != null && int.TryParse(param.ToString(), out pid)))
            {
                await SystemOptimizerService.EndProcessTaskAsync(pid);
                await RefreshOptimizerAsync();
            }
        });

        OptimizeRamCommand = new RelayCommand(async _ =>
        {
            await SystemOptimizerService.OptimizeRAMAsync();
            ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "RAM Booster", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "🚀 [MANUAL RAM BOOST] Đã giải phóng Standby List và tối ưu hóa RAM siêu tốc!", true, "RAM"));
            await RefreshOptimizerAsync();
        });

        ScanJunkCommand = new RelayCommand(async _ =>
        {
            if (IsScanningJunk) return;
            IsScanningJunk = true;
            try
            {
                JunkResult = await SystemOptimizerService.ScanSafeJunkAsync();
            }
            finally
            {
                IsScanningJunk = false;
            }
        }, _ => !IsScanningJunk);

        CleanJunkCommand = new RelayCommand(async _ =>
        {
            if (IsScanningJunk) return;
            IsScanningJunk = true;
            try
            {
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "🧹 [JUNK CLEAN] Đang dọn sạch rác, cache Windows Update & kích hoạt Ultra COMPRESS...", true, "JUNK"));
                JunkResult = await SystemOptimizerService.CleanSafeJunkAsync();
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, $"✅ [JUNK CLEAN] Đã dọn sạch rác & tối ưu hóa siêu tiết kiệm dung lượng ổ C:!", true, "JUNK"));
            }
            finally
            {
                IsScanningJunk = false;
            }
        }, _ => !IsScanningJunk);

        UltraCompressCommand = new RelayCommand(async _ =>
        {
            if (IsScanningJunk) return;
            IsScanningJunk = true;
            try
            {
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "⚡ [ULTRA COMPRESS] Đang kích hoạt nén WOF CompactOS & WinSxS để tiết kiệm dung lượng ổ C:...", true, "COMPRESS"));
                string res = await SystemOptimizerService.RunUltraCompressModeAsync();
                JunkResult = await SystemOptimizerService.ScanSafeJunkAsync();
                ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, $"✅ [ULTRA COMPRESS] {res}", true, "COMPRESS"));
            }
            finally
            {
                IsScanningJunk = false;
            }
        }, _ => !IsScanningJunk);

        SpeedTestService.OnStatusChanged += (s, status) =>
        {
            SpeedTestStatus = status;
        };

        _autoSwitchEngine.OnSwitchEvent += (s, log) =>
        {
            ActivityLogs.Insert(0, log);
            _ = RefreshAsync();
        };

        // Load saved settings on init
        _ = InitializeSettingsAsync();
        _ = StartAutoRamCleanerLoopAsync();
        _ = StartAutoWifiMonitorLoopAsync();
    }

    private async Task InitializeSettingsAsync()
    {
        var loaded = await _settingsService.LoadSettingsAsync();
        CurrentSettings = loaded;
        Loc.SetLanguage(loaded.Language);
        OnPropertyChanged(nameof(Loc));
        OnThemeChangedRequest?.Invoke(this, loaded.Theme);
        await RefreshOptimizerAsync();
    }

    private async Task StartAutoRamCleanerLoopAsync()
    {
        while (true)
        {
            await Task.Delay(10000);
            try
            {
                if (CurrentSettings.AutoCleanRamEnabled && CurrentSettings.AutoCleanRamIntervalSeconds > 0)
                {
                    int delaySeconds = Math.Max(10, CurrentSettings.AutoCleanRamIntervalSeconds);
                    await Task.Delay(delaySeconds * 1000);
                    if (CurrentSettings.AutoCleanRamEnabled)
                    {
                        await SystemOptimizerService.OptimizeRAMAsync();
                        ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "RAM Booster", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "🚀 [AUTO RAM CLEAN] Đã tự động dọn dẹp RAM định kỳ theo đúng lịch trình!", true, "RAM"));
                        if (CurrentTabIndex == 1)
                        {
                            _ = RefreshOptimizerAsync();
                        }
                    }
                }
            }
            catch { }
        }
    }

    private async Task StartAutoWifiMonitorLoopAsync()
    {
        while (true)
        {
            await Task.Delay(5000);
            try
            {
                if (IsSmartSelectionEnabled && _autoSwitchEngine != null)
                {
                    await _autoSwitchEngine.EvaluateAndSwitchAsync();
                }
            }
            catch { }
        }
    }

    public async Task RefreshOptimizerAsync()
    {
        MemoryStatus = await SystemOptimizerService.GetMemoryStatusAsync();
        var procs = await SystemOptimizerService.GetTopProcessesAsync(30);
        
        TopProcesses.Clear();
        foreach (var p in procs)
        {
            TopProcesses.Add(p);
        }
    }

    public virtual async Task RefreshAsync()
    {
        CurrentStatus = await _wifiService.GetCurrentInterfaceAsync();
        var allNetworks = await _wifiService.ScanBSSIDsAsync(CurrentStatus?.Bssid);

        AuthorizedNetworks.Clear();
        AvailableNetworks.Clear();

        if (CurrentStatus != null && !string.IsNullOrEmpty(CurrentStatus.Ssid))
        {
            var auth = allNetworks
                .Where(n => string.Equals(n.Ssid, CurrentStatus.Ssid, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(n => n.Score)
                .ToList();

            bool anyCurrent = auth.Any(n => n.IsCurrentConnection);
            if (!anyCurrent && auth.Count > 0 && CurrentStatus != null)
            {
                var match = auth.FirstOrDefault(n => string.Equals(n.Bssid?.Replace("-", ":").Trim(), CurrentStatus.Bssid?.Replace("-", ":").Trim(), StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    match = auth.FirstOrDefault(n => n.Band == CurrentStatus.Band);
                }
                if (match == null)
                {
                    match = auth[0];
                }
                if (match != null)
                {
                    int index = auth.IndexOf(match);
                    auth[index] = match with { IsCurrentConnection = true };
                }
            }

            foreach (var n in auth)
            {
                AuthorizedNetworks.Add(n);
            }

            var avail = allNetworks
                .Where(n => !string.Equals(n.Ssid, CurrentStatus.Ssid, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(n => n.Score);
            foreach (var n in avail)
            {
                AvailableNetworks.Add(n);
            }

            if (CurrentStatus.Band == WiFiBand.Band5GHz || CurrentStatus.Band == WiFiBand.Band6GHz)
            {
                CurrentBandBadgeText = Loc.Badge5Ghz;
                CurrentBandBadgeColor = "#00F280"; // Neon Green
            }
            else if (CurrentStatus.Band == WiFiBand.Band24GHz)
            {
                CurrentBandBadgeText = Loc.Badge24Ghz;
                CurrentBandBadgeColor = "#FF9900"; // Orange Warning
            }
            else
            {
                CurrentBandBadgeText = "Unknown";
                CurrentBandBadgeColor = "#64748B"; // Muted Gray
            }
        }
        else
        {
            CurrentBandBadgeText = Loc.BadgeDisconnected;
            CurrentBandBadgeColor = "#FF3355"; // Red
        }

        if (IsSmartSelectionEnabled && CurrentStatus?.Band == WiFiBand.Band24GHz)
        {
            await _autoSwitchEngine.EvaluateAndSwitchAsync();
        }

        if (CurrentTabIndex == 1)
        {
            await RefreshOptimizerAsync();
        }
    }

    public virtual async Task ForceSwitchTo5GhzAsync()
    {
        if (CurrentStatus == null) return;
        var best5Ghz = AuthorizedNetworks
            .Where(n => n.Band == WiFiBand.Band5GHz || n.Band == WiFiBand.Band6GHz)
            .OrderByDescending(n => n.Score)
            .FirstOrDefault();

        if (best5Ghz != null)
        {
            ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, best5Ghz.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, best5Ghz.Bssid, best5Ghz.Band, $"🚀 [MANUAL 5GHZ] Đang khóa & kết nối ngay lập tức sang điểm truy cập 5GHz ({best5Ghz.Bssid} - {best5Ghz.SignalQuality}%)...", true, "WIFI"));
            await _wifiService.ConnectToBSSIDAsync(best5Ghz.Ssid, best5Ghz.Bssid);
            ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, best5Ghz.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, best5Ghz.Bssid, best5Ghz.Band, $"✅ [MANUAL 5GHZ] Đã hoàn tất khóa BSSID & chuyển sang băng tần 5GHz!", true, "WIFI"));
            await RefreshAsync();
        }
        else
        {
            ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, CurrentStatus.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, CurrentStatus.Bssid, CurrentStatus.Band, "⚠️ [MANUAL 5GHZ] Không tìm thấy điểm truy cập 5GHz/6GHz nào cho mạng hiện tại!", false, "WIFI"));
        }
    }
}
