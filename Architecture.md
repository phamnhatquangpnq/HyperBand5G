# HyperBand 5G - System Architecture & Plan (.NET WPF Edition)

// Standardized to production level
// Purpose: Architecture definition, technical stack, TDD workflow, and feature roadmap for HyperBand 5G Desktop Application
// Dependencies: Windows 10/11, .NET 10.0 SDK, WPF, xUnit

## 1. Executive Summary & Technology Stack
To solve the frustrating problem of Windows laptops bouncing between 2.4GHz and 5GHz on combined (dual-band/mesh) SSIDs, we have built a **Production-Ready Windows Desktop Application** using **C# and .NET 10 WPF (Windows Presentation Foundation)**.

### Why C# & .NET WPF over Electron/Node.js?
1. **Direct Windows Native API & CLI Access**: Flawless interaction with Windows Native Wi-Fi APIs (`Wlanapi.dll`), `netsh wlan`, and Power Management without requiring cumbersome Node.js native wrappers or Python bridges.
2. **Zero Memory Bloat**: Electron uses ~150-300 MB RAM for a background system tray tool. A C# .NET WPF app consumes ~15-25 MB RAM and runs silently in the system tray with near-zero CPU overhead.
3. **Rich HyperBand 5G Aesthetics**: Using WPF XAML styling, we deliver a stunning, dark-mode, glassmorphic UI with customizable themes (HyperDark, CyberNeon, OLEDBlack).
4. **Clean Architecture & TDD**: Built with a decoupled core class library (`WifiBandLockPro.Core`) and UI presentation layer (`WifiBandLockPro.App`), fully tested via **xUnit** (`WifiBandLockPro.Tests`).

---

## 2. System Architecture & Project Structure

```
WIFA/
├── WifiBandLockPro.sln                     # C# .NET 10 Solution
├── .gitignore                              # C# / Visual Studio / .NET Gitignore
├── LICENSE                                 # MIT Open Source License for GitHub
├── README.md                               # Viral promotional GitHub Readme with download instructions
├── .github/
│   └── workflows/
│       └── release.yml                     # GitHub Actions CI/CD for auto-publishing executable releases
├── release/
│   └── HyperBand5G.exe                     # Standalone Self-Contained Single-File Executable (No .NET required)
├── src/
│   ├── WifiBandLockPro.Core/               # Core Domain & Logic (.NET 10 Library)
│   │   ├── Models/
│   │   │   ├── WiFiModels.cs               # Records/DTOs: BSSIDNetwork, WiFiInterfaceStatus, SwitchEventLog
│   │   │   ├── AppSettings.cs              # Application settings & preferences (Language, Theme, Startup)
│   │   │   └── SpeedTestModels.cs          # Records/DTOs for Wi-Fi Speed Test (Ping, Jitter, Download, Upload)
│   │   ├── Services/
│   │   │   ├── IWiFiService.cs             # Interface abstraction for network I/O & netsh commands
│   │   │   ├── WiFiService.cs              # Windows netsh command parser & band calculation
│   │   │   ├── AutoSwitchEngine.cs         # Autonomous monitoring & band steering engine
│   │   │   ├── ISettingsService.cs / .cs   # JSON settings storage & Registry Startup manager
│   │   │   ├── LocalizationService.cs      # MVVM real-time multi-language translation (VN/EN)
│   │   │   ├── ThemeService.cs             # Real-time WPF ResourceDictionary theme switcher
│   │   │   └── SpeedTestService.cs         # Built-in Wi-Fi speed test suite (Ping, Cloudflare CDN speed)
│   │   └── ViewModels/
│   │       ├── ViewModelBase.cs            # INotifyPropertyChanged base
│   │       ├── RelayCommand.cs             # ICommand implementation
│   │       └── MainViewModel.cs            # MVVM ViewModel coordinating UI state, speed test, & networks
│   └── WifiBandLockPro.App/                # Presentation Layer (.NET 10 WPF Application)
│       ├── app.ico                         # Custom Cyberpunk Neon Wi-Fi Radar application icon
│       ├── App.xaml / App.xaml.cs          # Application resources, dark theme, & System Tray NotifyIcon
│       ├── MainWindow.xaml / .xaml.cs      # HyperBand 5G dashboard UI with Settings, Speed Test, & Language
│       └── Converters/
│           └── ValueConverters.cs          # XAML value converters for badges & toggles
└── tests/
    └── WifiBandLockPro.Tests/              # TDD Test Suite (xUnit)
        ├── WiFiServiceTests.cs             # Unit tests for command parsing & band math
        ├── AutoSwitchEngineTests.cs        # Unit tests for autonomous band switching logic
        ├── MainViewModelTests.cs           # Unit tests for ViewModel data binding
        ├── LocalizationAndSettingsTests.cs # Unit tests for localization, string formatting, and settings
        ├── SpeedTestAndThemeTests.cs       # Unit tests for speed test math, branding, and theme switching
        └── PackagingAndGitHubTests.cs      # Unit tests for assembly packaging metadata and release verification
```

---

## 3. TDD & Autonomous Implementation Roadmap

### Phase 1 to Phase 8 (Completed 100%)
- [x] **Phase 1**: Core Domain Models & Netsh Command Parsing.
- [x] **Phase 2**: Autonomous Band Steering Engine.
- [x] **Phase 3**: MVVM ViewModel & UI Data Binding.
- [x] **Phase 4**: Rich WPF XAML UI & System Tray Integration.
- [x] **Phase 5**: Verification & Production Polish.
- [x] **Phase 6**: Multi-Language (EN/VN), Settings & Auto-Start, UI Refinements.
- [x] **Phase 7**: App Rebranding, Real-Time Theme & Language Switching, Built-In Speed Test Suite.
- [x] **Phase 8**: Standalone Executable Packaging (`HyperBand5G.exe` in `/release`), Custom Icon (`app.ico`), and GitHub Viral Readiness (.gitignore, LICENSE, CI/CD workflow, README.md).
