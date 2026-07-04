// Standardized to production level
// Purpose: Main ViewModel coordinating UI state, network tables, speed test, localization, and theme switching
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
    public LocalizationService Loc { get; }

    public event EventHandler<string>? OnThemeChangedRequest;

    private bool _isSmartSelectionEnabled;
    private int _currentTabIndex; // 0 = Dashboard, 1 = Speed Test, 2 = Settings
    private WiFiInterfaceStatus? _currentStatus;
    private string _currentBandBadgeText = "Scanning...";
    private string _currentBandBadgeColor = "#64748B"; // Muted gray
    private AppSettings _currentSettings = new();
    private SpeedTestStatus _speedTestStatus = new(false, "Ready / Sẵn sàng", 0, 0, 0, 0, 0);

    public ObservableCollection<BSSIDNetwork> AuthorizedNetworks { get; } = new();
    public ObservableCollection<BSSIDNetwork> AvailableNetworks { get; } = new();
    public ObservableCollection<SwitchEventLog> ActivityLogs { get; } = new();

    public ICommand ToggleSmartSelectionCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ForceSwitch5GhzCommand { get; }
    public ICommand SwitchTabCommand { get; }
    public ICommand SwitchLanguageCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand StartSpeedTestCommand { get; }

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
        set => SetProperty(ref _currentTabIndex, value);
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

    public MainViewModel(IWiFiService wifiService, AutoSwitchEngine autoSwitchEngine, ISettingsService? settingsService = null, LocalizationService? locService = null, ISpeedTestService? speedTestService = null)
    {
        _wifiService = wifiService ?? throw new ArgumentNullException(nameof(wifiService));
        _autoSwitchEngine = autoSwitchEngine ?? throw new ArgumentNullException(nameof(autoSwitchEngine));
        _settingsService = settingsService ?? new SettingsService();
        Loc = locService ?? new LocalizationService("vn");
        SpeedTestService = speedTestService ?? new SpeedTestService();

        _isSmartSelectionEnabled = _autoSwitchEngine.Config.Enabled;

        ToggleSmartSelectionCommand = new RelayCommand(_ => IsSmartSelectionEnabled = !IsSmartSelectionEnabled);
        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        ForceSwitch5GhzCommand = new RelayCommand(async _ => await ForceSwitchTo5GhzAsync());
        
        SwitchTabCommand = new RelayCommand(param =>
        {
            if (int.TryParse(param?.ToString(), out int idx))
            {
                CurrentTabIndex = idx;
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
    }

    private async Task InitializeSettingsAsync()
    {
        var loaded = await _settingsService.LoadSettingsAsync();
        CurrentSettings = loaded;
        Loc.SetLanguage(loaded.Language);
        OnPropertyChanged(nameof(Loc));
        OnThemeChangedRequest?.Invoke(this, loaded.Theme);
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
                .OrderByDescending(n => n.Score);
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
            await _wifiService.ConnectToBSSIDAsync(best5Ghz.Ssid, best5Ghz.Bssid);
            await RefreshAsync();
        }
    }
}
