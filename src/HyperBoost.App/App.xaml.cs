// Standardized to production level
// Purpose: Application entry point initializing background services, System Tray NotifyIcon, and dynamic theme switcher
// Dependencies: System.Windows, System.Windows.Forms, System.Windows.Media, System.Windows.Threading, HyperBoost.Core.Services, HyperBoost.Core.ViewModels

namespace HyperBoost.App;

using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using HyperBoost.Core.Services;
using HyperBoost.Core.ViewModels;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private WiFiService? _wifiService;
    private AutoSwitchEngine? _autoSwitchEngine;
    private SettingsService? _settingsService;
    private LocalizationService? _locService;
    private SpeedTestService? _speedTestService;
    private MainViewModel? _viewModel;
    private DispatcherTimer? _pollTimer;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _wifiService = new WiFiService();
        _autoSwitchEngine = new AutoSwitchEngine(_wifiService);
        _settingsService = new SettingsService();
        _locService = new LocalizationService("vn");
        _speedTestService = new SpeedTestService();
        _viewModel = new MainViewModel(_wifiService, _autoSwitchEngine, _settingsService, _locService, _speedTestService);

        // Listen for Theme Switch requests from ViewModel
        _viewModel.OnThemeChangedRequest += (s, themeName) =>
        {
            ApplyThemeToApp(themeName);
        };

        // Initialize System Tray Icon for silent background operation
        InitializeSystemTray();

        // Listen for switch events to trigger Toast Notification
        _autoSwitchEngine.OnSwitchEvent += (s, log) =>
        {
            if (log.Success)
            {
                _notifyIcon?.ShowBalloonTip(
                    4000,
                    _locService.IsVietnamese ? "HyperBoost 5G - Đã chuyển băng tần!" : "HyperBoost 5G - Switched Band!",
                    _locService.IsVietnamese 
                        ? $"Tự động chuyển từ 2.4 GHz sang 5 GHz (BSSID: {log.ToBssid}) trên Wi-Fi '{log.Ssid}'."
                        : $"Automatically switched from 2.4 GHz to 5 GHz BSSID ({log.ToBssid}) on network '{log.Ssid}'.",
                    ToolTipIcon.Info
                );
            }
        };

        // Create and show MainWindow
        var mainWindow = new MainWindow(_viewModel);
        MainWindow = mainWindow;
        mainWindow.Show();

        // Perform initial network scan
        await _viewModel.RefreshAsync();

        // Start background polling loop
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Math.Max(1, _viewModel.CurrentSettings.PollIntervalSeconds))
        };
        _pollTimer.Tick += async (s, args) =>
        {
            if (_viewModel != null)
            {
                _pollTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, _viewModel.CurrentSettings.PollIntervalSeconds));
                await _viewModel.RefreshAsync();
            }
        };
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.PollIntervalSeconds) || e.PropertyName == nameof(_viewModel.CurrentSettings))
            {
                _pollTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, _viewModel.CurrentSettings.PollIntervalSeconds));
            }
        };
        _pollTimer.Start();
    }

    public static void ApplyThemeToApp(string themeName)
    {
        var colors = ThemeService.GetThemeColors(themeName);
        var res = Current.Resources;
        res["KillerDarkBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Background));
        res["KillerPanelBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Panel));
        res["KillerBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Border));
        res["KillerAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Accent));
        res["KillerGreenBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Green));
        res["KillerTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Text));
        res["KillerMutedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Muted));
    }

    private void InitializeSystemTray()
    {
        Icon? appIcon = null;
        try
        {
            string? exeLocation = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exeLocation))
            {
                appIcon = Icon.ExtractAssociatedIcon(exeLocation);
            }
        }
        catch { }

        if (appIcon == null)
        {
            try
            {
                var uri = new Uri("pack://application:,,,/app.ico");
                var streamInfo = GetResourceStream(uri);
                if (streamInfo != null)
                {
                    appIcon = new Icon(streamInfo.Stream);
                }
            }
            catch { }
        }

        _notifyIcon = new NotifyIcon
        {
            Text = "HyperBoost 5G & PC Suite",
            Icon = appIcon ?? SystemIcons.Shield,
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        
        var openItem = new ToolStripMenuItem("Open HyperBoost 5G & PC Suite / Mở Giao Diện");
        openItem.Click += (s, args) => ShowMainWindow();
        contextMenu.Items.Add(openItem);

        var toggleItem = new ToolStripMenuItem("Toggle Smart 5GHz Lock / Bật Tắt Khóa 5GHz");
        toggleItem.Click += (s, args) =>
        {
            if (_viewModel != null)
            {
                _viewModel.IsSmartSelectionEnabled = !_viewModel.IsSmartSelectionEnabled;
                _notifyIcon.ShowBalloonTip(2000, "HyperBoost 5G & PC Suite", $"Smart Lock: {(_viewModel.IsSmartSelectionEnabled ? "ENABLED / BẬT" : "DISABLED / TẮT")}", ToolTipIcon.Info);
            }
        };
        contextMenu.Items.Add(toggleItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit / Thoát");
        exitItem.Click += (s, args) =>
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            Shutdown();
        };
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (MainWindow != null)
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pollTimer?.Stop();
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.OnExit(e);
    }
}
