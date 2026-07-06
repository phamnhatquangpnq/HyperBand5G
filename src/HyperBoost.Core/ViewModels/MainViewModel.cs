using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HyperBoost.Core.Models;
using HyperBoost.Core.Services;

namespace HyperBoost.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
	private readonly IWiFiService _wifiService;

	private readonly AutoSwitchEngine _autoSwitchEngine;

	private readonly ISettingsService _settingsService;

	private bool _isSmartSelectionEnabled;

	private int _currentTabIndex;

	private WiFiInterfaceStatus? _currentStatus;

	private string _currentBandBadgeText = "Scanning...";

	private string _currentBandBadgeColor = "#64748B";

	private AppSettings _currentSettings = new AppSettings();

	private SpeedTestStatus _speedTestStatus = new SpeedTestStatus(IsRunning: false, "Ready / Sẵn sàng", 0, 0, 0.0, 0.0, 0.0);

	private SystemMemoryStatus _memoryStatus = new SystemMemoryStatus(17179869184uL, 8589934592uL, 8589934592uL, 50);

	private JunkScanResult _junkResult = new JunkScanResult(0, 0L, 0, 0L, 0, 0L, 0, 0L, 0, 0L);

	private bool _isScanningJunk;

	private bool _isLoadingApps;

	private bool _isOptimizingRam;

	private bool _isUninstalling;

	private string _selectedLogFilter = "Tất Cả (All)";

	public ISpeedTestService SpeedTestService { get; }

	public ISystemOptimizerService SystemOptimizerService { get; }

	public LocalizationService Loc { get; }

	public ObservableCollection<BSSIDNetwork> AuthorizedNetworks { get; } = new ObservableCollection<BSSIDNetwork>();

	public ObservableCollection<BSSIDNetwork> AvailableNetworks { get; } = new ObservableCollection<BSSIDNetwork>();

	public ObservableCollection<SwitchEventLog> ActivityLogs { get; } = new ObservableCollection<SwitchEventLog>();

	public ObservableCollection<ProcessMemoryItem> TopProcesses { get; } = new ObservableCollection<ProcessMemoryItem>();

	public ObservableCollection<InstalledAppItem> InstalledApps { get; } = new ObservableCollection<InstalledAppItem>();

	public ISoftwareUninstallerService UninstallerService { get; }

	public bool IsLoadingApps
	{
		get
		{
			return _isLoadingApps;
		}
		set
		{
			SetProperty(ref _isLoadingApps, value, "IsLoadingApps");
			OnPropertyChanged("IsNotLoadingApps");
		}
	}
	public bool IsNotLoadingApps => !IsLoadingApps;

	public bool IsOptimizingRam
	{
		get
		{
			return _isOptimizingRam;
		}
		set
		{
			SetProperty(ref _isOptimizingRam, value, "IsOptimizingRam");
			OnPropertyChanged("IsNotOptimizingRam");
		}
	}
	public bool IsNotOptimizingRam => !IsOptimizingRam;

	public bool IsUninstalling
	{
		get
		{
			return _isUninstalling;
		}
		set
		{
			SetProperty(ref _isUninstalling, value, "IsUninstalling");
			OnPropertyChanged("IsNotUninstalling");
		}
	}
	public bool IsNotUninstalling => !IsUninstalling;

	public ObservableCollection<string> LogFilters { get; } = new ObservableCollection<string> { "Tất Cả (All)", "\ud83d\udce1 Wi-Fi", "\ud83d\ude80 RAM Boost", "\ud83e\uddf9 Junk Clean", "⚡ Compress", "\ud83d\uddd1\ufe0f Uninstall", "⚡ Speed Test" };

	public string SelectedLogFilter
	{
		get
		{
			return _selectedLogFilter;
		}
		set
		{
			if (SetProperty(ref _selectedLogFilter, value, "SelectedLogFilter"))
			{
				ApplyLogFilter();
			}
		}
	}

	public ObservableCollection<SwitchEventLog> FilteredActivityLogs { get; } = new ObservableCollection<SwitchEventLog>();

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

	public ICommand RefreshAppsCommand { get; }

	public ICommand UninstallAppCommand { get; }

	public ICommand OpenInstallLocationCommand { get; }

	public ICommand ShowAppInfoCommand { get; }

	public ICommand ToggleLogVisibilityCommand { get; }

	private bool _isLogVisible = true;
	public bool IsLogVisible
	{
		get => _isLogVisible;
		set
		{
			SetProperty(ref _isLogVisible, value, "IsLogVisible");
			OnPropertyChanged("LogToggleBtnText");
		}
	}
	public string LogToggleBtnText => IsLogVisible ? "▲ Ẩn nhật ký hoạt động" : "▼ Bật xem nhật ký hoạt động";

	private string _liveJunkLogText = "Sẵn sàng dọn dẹp & tối ưu hóa hệ thống...";
	public string LiveJunkLogText
	{
		get => _liveJunkLogText;
		set => SetProperty(ref _liveJunkLogText, value, "LiveJunkLogText");
	}

	public bool IsSmartSelectionEnabled
	{
		get
		{
			return _isSmartSelectionEnabled;
		}
		set
		{
			if (SetProperty(ref _isSmartSelectionEnabled, value, "IsSmartSelectionEnabled"))
			{
				_autoSwitchEngine.Config = _autoSwitchEngine.Config with
				{
					Enabled = value
				};
			}
		}
	}

	public int CurrentTabIndex
	{
		get
		{
			return _currentTabIndex;
		}
		set
		{
			if (SetProperty(ref _currentTabIndex, value, "CurrentTabIndex"))
			{
				switch (value)
				{
				case 1:
					RefreshOptimizerAsync();
					break;
				case 2:
					LoadInstalledAppsAsync();
					break;
				}
			}
		}
	}

	public WiFiInterfaceStatus? CurrentStatus
	{
		get
		{
			return _currentStatus;
		}
		set
		{
			SetProperty(ref _currentStatus, value, "CurrentStatus");
		}
	}

	public string CurrentBandBadgeText
	{
		get
		{
			return _currentBandBadgeText;
		}
		set
		{
			SetProperty(ref _currentBandBadgeText, value, "CurrentBandBadgeText");
		}
	}

	public string CurrentBandBadgeColor
	{
		get
		{
			return _currentBandBadgeColor;
		}
		set
		{
			SetProperty(ref _currentBandBadgeColor, value, "CurrentBandBadgeColor");
		}
	}

	public AppSettings CurrentSettings
	{
		get
		{
			return _currentSettings;
		}
		set
		{
			if (SetProperty(ref _currentSettings, value, "CurrentSettings"))
			{
				_autoSwitchEngine.Config = _autoSwitchEngine.Config with
				{
					PollIntervalMs = value.PollIntervalSeconds * 1000,
					Min5GhzQualityThreshold = value.Min5GhzQualityThreshold
				};
				this.OnThemeChangedRequest?.Invoke(this, value.Theme);
				OnPropertyChanged("SelectedTheme");
				OnPropertyChanged("RunOnStartup");
				OnPropertyChanged("PollIntervalSeconds");
				OnPropertyChanged("Min5GhzQualityThreshold");
				OnPropertyChanged("AutoCleanRamEnabled");
				OnPropertyChanged("AutoCleanRamIntervalSeconds");
			}
		}
	}

	public string SelectedTheme
	{
		get
		{
			return CurrentSettings.Theme;
		}
		set
		{
			if (!string.Equals(CurrentSettings.Theme, value, StringComparison.OrdinalIgnoreCase))
			{
				CurrentSettings = CurrentSettings with
				{
					Theme = value
				};
				OnPropertyChanged("SelectedTheme");
				this.OnThemeChangedRequest?.Invoke(this, value);
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public bool RunOnStartup
	{
		get => CurrentSettings.RunOnStartup;
		set
		{
			if (CurrentSettings.RunOnStartup != value)
			{
				CurrentSettings = CurrentSettings with { RunOnStartup = value };
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public int PollIntervalSeconds
	{
		get => CurrentSettings.PollIntervalSeconds;
		set
		{
			if (CurrentSettings.PollIntervalSeconds != value)
			{
				CurrentSettings = CurrentSettings with { PollIntervalSeconds = value };
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public int Min5GhzQualityThreshold
	{
		get => CurrentSettings.Min5GhzQualityThreshold;
		set
		{
			if (CurrentSettings.Min5GhzQualityThreshold != value)
			{
				CurrentSettings = CurrentSettings with { Min5GhzQualityThreshold = value };
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public bool AutoCleanRamEnabled
	{
		get => CurrentSettings.AutoCleanRamEnabled;
		set
		{
			if (CurrentSettings.AutoCleanRamEnabled != value)
			{
				CurrentSettings = CurrentSettings with { AutoCleanRamEnabled = value };
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public int AutoCleanRamIntervalSeconds
	{
		get => CurrentSettings.AutoCleanRamIntervalSeconds;
		set
		{
			if (CurrentSettings.AutoCleanRamIntervalSeconds != value)
			{
				CurrentSettings = CurrentSettings with { AutoCleanRamIntervalSeconds = value };
				_settingsService.SaveSettingsAsync(CurrentSettings);
			}
		}
	}

	public SpeedTestStatus SpeedTestStatus
	{
		get
		{
			return _speedTestStatus;
		}
		set
		{
			SetProperty(ref _speedTestStatus, value, "SpeedTestStatus");
		}
	}

	public SystemMemoryStatus MemoryStatus
	{
		get
		{
			return _memoryStatus;
		}
		set
		{
			SetProperty(ref _memoryStatus, value, "MemoryStatus");
		}
	}

	public JunkScanResult JunkResult
	{
		get
		{
			return _junkResult;
		}
		set
		{
			SetProperty(ref _junkResult, value, "JunkResult");
		}
	}

	public bool IsScanningJunk
	{
		get
		{
			return _isScanningJunk;
		}
		set
		{
			SetProperty(ref _isScanningJunk, value, "IsScanningJunk");
			OnPropertyChanged("IsNotScanningJunk");
		}
	}
	public bool IsNotScanningJunk => !IsScanningJunk;

	public event EventHandler<string>? OnThemeChangedRequest;

	public event EventHandler<InstalledAppItem>? OnShowAppInfoRequest;

	private void ApplyLogFilter()
	{
		FilteredActivityLogs.Clear();
		foreach (SwitchEventLog activityLog in ActivityLogs)
		{
			if (MatchesFilter(activityLog))
			{
				FilteredActivityLogs.Add(activityLog);
			}
		}
	}

	private bool MatchesFilter(SwitchEventLog log)
	{
		if (string.IsNullOrWhiteSpace(_selectedLogFilter) || _selectedLogFilter.StartsWith("Tất Cả") || _selectedLogFilter.StartsWith("All"))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("Wi-Fi") && (log.Category == "WIFI" || log.CategoryBadge.Contains("Wi-Fi")))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("RAM") && (log.Category == "RAM" || log.CategoryBadge.Contains("RAM")))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("Junk") && (log.Category == "JUNK" || log.CategoryBadge.Contains("Junk")))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("Compress") && (log.Category == "COMPRESS" || log.CategoryBadge.Contains("Compress")))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("Uninstall") && (log.Category == "UNINSTALL" || log.CategoryBadge.Contains("Uninstall")))
		{
			return true;
		}
		if (_selectedLogFilter.Contains("Speed") && (log.Category == "SPEED" || log.CategoryBadge.Contains("Speed")))
		{
			return true;
		}
		return false;
	}

	public MainViewModel(IWiFiService wifiService, AutoSwitchEngine autoSwitchEngine, ISettingsService? settingsService = null, LocalizationService? locService = null, ISpeedTestService? speedTestService = null, ISystemOptimizerService? optimizerService = null)
	{
		_wifiService = wifiService ?? throw new ArgumentNullException("wifiService");
		_autoSwitchEngine = autoSwitchEngine ?? throw new ArgumentNullException("autoSwitchEngine");
		_settingsService = settingsService ?? new SettingsService();
		Loc = locService ?? new LocalizationService();
		SpeedTestService = speedTestService ?? new SpeedTestService();
		SystemOptimizerService = optimizerService ?? new SystemOptimizerService();
		SystemOptimizerService.OnCleaningProgress += (s, msg) =>
		{
			System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() =>
			{
				LiveJunkLogText = msg;
			});
		};
		UninstallerService = new SoftwareUninstallerService();
		ActivityLogs.CollectionChanged += delegate
		{
			ApplyLogFilter();
		};
		_isSmartSelectionEnabled = _autoSwitchEngine.Config.Enabled;
		ToggleLogVisibilityCommand = new RelayCommand(delegate
		{
			IsLogVisible = !IsLogVisible;
		});
		ToggleSmartSelectionCommand = new RelayCommand(async delegate
		{
			IsSmartSelectionEnabled = !IsSmartSelectionEnabled;
			if (!IsSmartSelectionEnabled)
			{
				await _wifiService.ResetAdapterPreferredBandAsync();
			}
			else
			{
				await _wifiService.SetAdapterPreferredBand5GHzAsync();
				if (_autoSwitchEngine != null)
				{
					await _autoSwitchEngine.EvaluateAndSwitchAsync();
				}
			}
			ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "Wi-Fi Steering", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "\ud83d\udd04 [SMART STEERING] Chế độ tự động khóa & điều hướng 5GHz: " + (IsSmartSelectionEnabled ? "BẬT (ACTIVE - ÉP 5GHZ)" : "TẮT (AUTO / KHÔNG ÉP)"), Success: true));
		});
		RefreshCommand = new RelayCommand(async delegate
		{
			await RefreshAsync();
		});
		ForceSwitch5GhzCommand = new RelayCommand(async delegate
		{
			await ForceSwitchTo5GhzAsync();
		});
		ConnectToBssidCommand = new RelayCommand(async delegate(object? param)
		{
			if (param is BSSIDNetwork target && CurrentStatus != null)
			{
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, target.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, target.Bssid, target.Band, $"\ud83d\ude80 [MANUAL SWITCH] Đang kết nối tới điểm truy cập BSSID {target.Bssid} ({target.BandDisplay} - {target.SignalQuality}%)...", Success: true));
				await _wifiService.ConnectToBSSIDAsync(target.Ssid, target.Bssid);
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, target.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, target.Bssid, target.Band, $"✅ [MANUAL SWITCH] Đã kết nối thành công BSSID {target.Bssid} ({target.BandDisplay})!", Success: true));
				await RefreshAsync();
			}
		});
		ClearLogsCommand = new RelayCommand(delegate
		{
			ActivityLogs.Clear();
		});
		SwitchTabCommand = new RelayCommand(delegate(object? param)
		{
			if (int.TryParse(param?.ToString(), out var result))
			{
				CurrentTabIndex = result;
			}
		});
		SwitchLanguageCommand = new RelayCommand(async delegate(object? param)
		{
			string language = (param as string) ?? "vn";
			Loc.SetLanguage(language);
			OnPropertyChanged("Loc");
			CurrentSettings = CurrentSettings with
			{
				Language = language
			};
			await _settingsService.SaveSettingsAsync(CurrentSettings);
			await RefreshAsync();
		});
		SaveSettingsCommand = new RelayCommand(async delegate
		{
			await _settingsService.SaveSettingsAsync(CurrentSettings);
			this.OnThemeChangedRequest?.Invoke(this, CurrentSettings.Theme);
			CurrentTabIndex = 0;
		});
		StartSpeedTestCommand = new RelayCommand(async delegate
		{
			if (!SpeedTestStatus.IsRunning)
			{
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "Speed Test", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "⚡ [SPEED TEST] Đang kiểm tra độ trễ Ping, Jitter và tốc độ tải Direct Server & Cloud CDN...", Success: true, "SPEED"));
				SpeedTestStatus = await SpeedTestService.RunSpeedTestAsync();
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "Speed Test", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, $"✅ [SPEED TEST] Hoàn tất! Ping: {SpeedTestStatus.PingMs}ms | Down Direct: {SpeedTestStatus.DownloadMbps:F1} Mbps / Cloud: {SpeedTestStatus.CloudDownloadMbps:F1} Mbps | Up Direct: {SpeedTestStatus.UploadMbps:F1} Mbps / Cloud: {SpeedTestStatus.CloudUploadMbps:F1} Mbps", Success: true, "SPEED"));
			}
		}, (object? _) => !SpeedTestStatus.IsRunning);
		RefreshOptimizerCommand = new RelayCommand(async delegate
		{
			await RefreshOptimizerAsync();
		});
		EndTaskCommand = new RelayCommand(async delegate(object? param)
		{
			int result;
			if (param is int)
			{
				result = (int)param;
			}
			else if (param == null || !int.TryParse(param.ToString(), out result))
			{
				return;
			}
			await SystemOptimizerService.EndProcessTaskAsync(result);
			await RefreshOptimizerAsync();
		});
		OptimizeRamCommand = new RelayCommand(async delegate
		{
			if (IsOptimizingRam)
			{
				return;
			}
			IsOptimizingRam = true;
			try
			{
				await SystemOptimizerService.OptimizeRAMAsync();
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "RAM Booster", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "\ud83d\ude80 [MANUAL RAM BOOST] Đã giải phóng Standby List và tối ưu hóa RAM siêu tốc!", Success: true, "RAM"));
				await RefreshOptimizerAsync();
			}
			finally
			{
				IsOptimizingRam = false;
			}
		}, (object? _) => !IsOptimizingRam);
		ScanJunkCommand = new RelayCommand(async delegate
		{
			if (IsScanningJunk)
			{
				return;
			}
			IsScanningJunk = true;
			try
			{
				LiveJunkLogText = "Đang quét các thư mục rác hệ thống...";
				JunkResult = await SystemOptimizerService.ScanSafeJunkAsync();
				LiveJunkLogText = $"Quét hoàn tất: Phát hiện {JunkResult.TotalFilesFound} file ({JunkResult.TotalSizeDisplay})";
			}
			finally
			{
				IsScanningJunk = false;
			}
		}, (object? _) => !IsScanningJunk);
		CleanJunkCommand = new RelayCommand(async delegate
		{
			if (IsScanningJunk)
			{
				return;
			}
			IsScanningJunk = true;
			try
			{
				LiveJunkLogText = "Đang bắt đầu dọn sạch rác hệ thống & nén CompactOS...";
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "\ud83e\uddf9 [JUNK CLEAN] Đang dọn sạch rác, cache Windows Update & kích hoạt Ultra COMPRESS...", Success: true, "JUNK"));
				JunkResult = await SystemOptimizerService.CleanSafeJunkAsync();
				LiveJunkLogText = $"✅ Dọn dẹp thành công! Đã giải phóng {JunkResult.FreedDisplay} dung lượng ổ C:";
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, $"✅ [JUNK CLEAN] Hoàn tất! Đã dọn sạch {JunkResult.FreedDisplay} rác hệ thống & tối ưu hóa siêu tiết kiệm ổ C:!", Success: true, "JUNK"));
			}
			finally
			{
				IsScanningJunk = false;
			}
		}, (object? _) => !IsScanningJunk);
		UltraCompressCommand = new RelayCommand(async delegate
		{
			if (IsScanningJunk)
			{
				return;
			}
			IsScanningJunk = true;
			try
			{
				LiveJunkLogText = "⚡ Đang kích hoạt Ultra COMPRESS (nén WOF CompactOS & WinSxS)...";
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "⚡ [ULTRA COMPRESS] Đang kích hoạt nén WOF CompactOS & WinSxS để tiết kiệm dung lượng ổ C:...", Success: true, "COMPRESS"));
				string res = await SystemOptimizerService.RunUltraCompressModeAsync();
				JunkResult = await SystemOptimizerService.ScanSafeJunkAsync();
				LiveJunkLogText = $"✅ {res}";
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "System Optimizer", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "✅ [ULTRA COMPRESS] " + res, Success: true, "COMPRESS"));
			}
			finally
			{
				IsScanningJunk = false;
			}
		}, (object? _) => !IsScanningJunk);
		RefreshAppsCommand = new RelayCommand(async delegate
		{
			await LoadInstalledAppsAsync();
		}, (object? _) => !IsLoadingApps);
		UninstallAppCommand = new RelayCommand(async delegate(object? param)
		{
			if (IsUninstalling || !(param is InstalledAppItem installedAppItem))
			{
				return;
			}
			IsUninstalling = true;
			try
			{
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "Uninstaller", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "\ud83d\uddd1\ufe0f [UNINSTALL] Đang gỡ cài đặt: " + installedAppItem.DisplayName + "...", Success: true, "UNINSTALL"));
				await UninstallerService.UninstallAppAsync(installedAppItem);
			}
			finally
			{
				IsUninstalling = false;
			}
		}, (object? _) => !IsUninstalling);
		OpenInstallLocationCommand = new RelayCommand(async delegate(object? param)
		{
			if (param is InstalledAppItem app)
			{
				await UninstallerService.OpenInstallLocationAsync(app);
			}
		});
		ShowAppInfoCommand = new RelayCommand(delegate(object? param)
		{
			if (param is InstalledAppItem e)
			{
				this.OnShowAppInfoRequest?.Invoke(this, e);
			}
		});
		SpeedTestService.OnStatusChanged += delegate(object? s, SpeedTestStatus status)
		{
			SpeedTestStatus = status;
		};
		_autoSwitchEngine.OnSwitchEvent += delegate(object? s, SwitchEventLog log)
		{
			ActivityLogs.Insert(0, log);
			RefreshAsync();
		};
		InitializeSettingsAsync();
		StartAutoRamCleanerLoopAsync();
		StartAutoWifiMonitorLoopAsync();
	}

	private async Task InitializeSettingsAsync()
	{
		AppSettings appSettings = (CurrentSettings = await _settingsService.LoadSettingsAsync());
		Loc.SetLanguage(appSettings.Language);
		OnPropertyChanged("Loc");
		this.OnThemeChangedRequest?.Invoke(this, appSettings.Theme);
		await RefreshOptimizerAsync();
	}

	private async Task StartAutoRamCleanerLoopAsync()
	{
		await Task.Delay(5000);
		int elapsedSeconds = 0;
		while (true)
		{
			await Task.Delay(1000);
			try
			{
				if (!CurrentSettings.AutoCleanRamEnabled || CurrentSettings.AutoCleanRamIntervalSeconds <= 0)
				{
					elapsedSeconds = 0;
					continue;
				}
				elapsedSeconds++;
				if (elapsedSeconds >= Math.Max(10, CurrentSettings.AutoCleanRamIntervalSeconds))
				{
					elapsedSeconds = 0;
					if (CurrentSettings.AutoCleanRamEnabled)
					{
						await SystemOptimizerService.OptimizeRAMAsync();
						ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, "RAM Booster", "-", WiFiBand.Band5GHz, "-", WiFiBand.Band5GHz, "\ud83d\ude80 [AUTO RAM CLEAN] Đã tự động dọn dẹp RAM định kỳ theo đúng lịch trình!", Success: true, "RAM"));
						if (CurrentTabIndex == 1)
						{
							RefreshOptimizerAsync();
						}
					}
				}
			}
			catch
			{
			}
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
			catch
			{
			}
		}
	}

	public async Task RefreshOptimizerAsync()
	{
		MemoryStatus = await SystemOptimizerService.GetMemoryStatusAsync();
		List<ProcessMemoryItem> obj = await SystemOptimizerService.GetTopProcessesAsync(30);
		TopProcesses.Clear();
		foreach (ProcessMemoryItem item in obj)
		{
			TopProcesses.Add(item);
		}
	}

	public virtual async Task RefreshAsync()
	{
		CurrentStatus = await _wifiService.GetCurrentInterfaceAsync();
		List<BSSIDNetwork> source = await _wifiService.ScanBSSIDsAsync(CurrentStatus?.Bssid);
		AuthorizedNetworks.Clear();
		AvailableNetworks.Clear();
		if (CurrentStatus != null && !string.IsNullOrEmpty(CurrentStatus.Ssid))
		{
			List<BSSIDNetwork> list = (from n in source
				where string.Equals(n.Ssid, CurrentStatus.Ssid, StringComparison.OrdinalIgnoreCase)
				orderby n.Score descending
				select n).ToList();
			if (!list.Any((BSSIDNetwork n) => n.IsCurrentConnection) && list.Count > 0 && CurrentStatus != null)
			{
				BSSIDNetwork bSSIDNetwork = list.FirstOrDefault((BSSIDNetwork n) => string.Equals(n.Bssid?.Replace("-", ":").Trim(), CurrentStatus.Bssid?.Replace("-", ":").Trim(), StringComparison.OrdinalIgnoreCase));
				if (bSSIDNetwork == null)
				{
					bSSIDNetwork = list.FirstOrDefault((BSSIDNetwork n) => n.Band == CurrentStatus.Band);
				}
				if (bSSIDNetwork == null)
				{
					bSSIDNetwork = list[0];
				}
				if (bSSIDNetwork != null)
				{
					int index = list.IndexOf(bSSIDNetwork);
					list[index] = bSSIDNetwork with
					{
						IsCurrentConnection = true
					};
				}
			}
			foreach (BSSIDNetwork item in list)
			{
				AuthorizedNetworks.Add(item);
			}
			foreach (BSSIDNetwork item2 in from n in source
				where !string.Equals(n.Ssid, CurrentStatus?.Ssid, StringComparison.OrdinalIgnoreCase)
				orderby n.Score descending
				select n)
			{
				AvailableNetworks.Add(item2);
			}
			WiFiInterfaceStatus? currentStatus = CurrentStatus;
			if ((object)currentStatus == null || currentStatus.Band != WiFiBand.Band5GHz)
			{
				WiFiInterfaceStatus? currentStatus2 = CurrentStatus;
				if ((object)currentStatus2 == null || currentStatus2.Band != WiFiBand.Band6GHz)
				{
					WiFiInterfaceStatus? currentStatus3 = CurrentStatus;
					if ((object)currentStatus3 != null && currentStatus3.Band == WiFiBand.Band24GHz)
					{
						CurrentBandBadgeText = Loc.Badge24Ghz;
						CurrentBandBadgeColor = "#FF9900";
					}
					else
					{
						CurrentBandBadgeText = "Unknown";
						CurrentBandBadgeColor = "#64748B";
					}
					goto IL_039f;
				}
			}
			CurrentBandBadgeText = Loc.Badge5Ghz;
			CurrentBandBadgeColor = "#00F280";
		}
		else
		{
			CurrentBandBadgeText = Loc.BadgeDisconnected;
			CurrentBandBadgeColor = "#FF3355";
		}
		goto IL_039f;
		IL_039f:
		if (IsSmartSelectionEnabled)
		{
			WiFiInterfaceStatus? currentStatus4 = CurrentStatus;
			if ((object)currentStatus4 != null && currentStatus4.Band == WiFiBand.Band24GHz)
			{
				await _autoSwitchEngine.EvaluateAndSwitchAsync();
			}
		}
		if (CurrentTabIndex == 1)
		{
			await RefreshOptimizerAsync();
		}
		else if (CurrentTabIndex == 2)
		{
			await LoadInstalledAppsAsync();
		}
	}

	public virtual async Task ForceSwitchTo5GhzAsync()
	{
		if (!(CurrentStatus == null))
		{
			BSSIDNetwork best5Ghz = (from n in AuthorizedNetworks
				where n.Band == WiFiBand.Band5GHz || n.Band == WiFiBand.Band6GHz
				orderby n.Score descending
				select n).FirstOrDefault();
			if (best5Ghz != null)
			{
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, best5Ghz.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, best5Ghz.Bssid, best5Ghz.Band, $"\ud83d\ude80 [MANUAL 5GHZ] Đang khóa & kết nối ngay lập tức sang điểm truy cập 5GHz ({best5Ghz.Bssid} - {best5Ghz.SignalQuality}%)...", Success: true));
				await _wifiService.ConnectToBSSIDAsync(best5Ghz.Ssid, best5Ghz.Bssid);
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, best5Ghz.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, best5Ghz.Bssid, best5Ghz.Band, "✅ [MANUAL 5GHZ] Đã hoàn tất khóa BSSID & chuyển sang băng tần 5GHz!", Success: true));
				await RefreshAsync();
			}
			else
			{
				ActivityLogs.Insert(0, new SwitchEventLog(Guid.NewGuid().ToString(), DateTime.Now, CurrentStatus.Ssid, CurrentStatus.Bssid, CurrentStatus.Band, CurrentStatus.Bssid, CurrentStatus.Band, "⚠\ufe0f [MANUAL 5GHZ] Không tìm thấy điểm truy cập 5GHz/6GHz nào cho mạng hiện tại!", Success: false));
			}
		}
	}

	public async Task LoadInstalledAppsAsync()
	{
		if (IsLoadingApps)
		{
			return;
		}
		IsLoadingApps = true;
		try
		{
			List<InstalledAppItem> obj = await UninstallerService.GetInstalledAppsAsync();
			InstalledApps.Clear();
			foreach (InstalledAppItem item in obj)
			{
				InstalledApps.Add(item);
			}
		}
		catch
		{
		}
		finally
		{
			IsLoadingApps = false;
		}
	}
}
