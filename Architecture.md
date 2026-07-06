# ARCHITECTURE & SYSTEM DESIGN: HYPERBOOST 5G & PC SUITE
**Version:** 2.0.0 (Production-Grade All-in-One Suite)  
**Status:** Stage 2 - Antigravity Beast Mode Active  
**Rebranding:** Renamed from *HyperBand 5G Suite* to **HyperBoost 5G & PC Suite**  

---

## 1. Executive Summary & Core Capabilities
**HyperBoost 5G & PC Suite** is a state-of-the-art Windows desktop application built with **.NET 10.0 WPF** and **Clean MVVM Architecture**. It combines autonomous Wi-Fi 5GHz band steering with real-time network diagnostics, cloud-edge speed testing, intelligent RAM optimization, visual Task Management, and safe system junk cleaning.

### Key Modules:
1. **Smart Wi-Fi Band Steering (`AutoSwitchEngine`)**: Automatically detects when Windows drops to congested 2.4GHz bands on dual-band/mesh networks and seamlessly forces connection back to high-speed 5GHz/6GHz BSSIDs.
2. **Real-Time Speed Test (`SpeedTestService`)**: Cloudflare CDN-backed latency (Ping), Jitter, Download, and Upload measurement.
3. **RAM Booster & Task Manager (`SystemOptimizerService`)**:
   - Real-time system memory monitoring (Total, Used %, Free GB via Win32 `GlobalMemoryStatusEx`).
   - Live Process List showing running applications, CPU/RAM working set (MB), and extracted **Application Icons** (via Win32 `SHGetFileInfo`/`ExtractIconEx`).
   - Interactive **End Task** capability (`process.Kill()`).
   - **Auto RAM Cleaner Timer**: Background execution every 60 seconds (or customized interval) using Win32 `EmptyWorkingSet` (`psapi.dll`) without freezing applications or causing spikes.
4. **Safe System Junk Cleaner (`SystemOptimizerService`)**:
   - Strictly safe cleaning protocol: Never modifies user documents, downloads, or software configuration.
   - 4-Tier Safe Targets: User Temp (`%TEMP%`), Windows Temp (`C:\Windows\Temp`), Windows Recycle Bin (`SHEmptyRecycleBin`), and obsolete system crash logs/dumps.
5. **Dynamic Theme & Localization Engine (`LocalizationService` & ResourceDictionaries)**: Real-time theme switching (HyperDark, CyberNeon, OLEDBlack) and instant Vietnamese/English bilingual toggling.
6. **Self-Contained Portable Packaging**: Zero-dependency single-file Windows executable (`HyperBoost.exe`) with custom embedded cybernetic Rocket/5G icon (`app.ico`).

---

## 2. Solution & Folder Structure
```
WIFA/
├── .github/
│   └── workflows/
│       └── release.yml                 # Automated CI/CD build & GitHub release
├── src/
│   ├── HyperBoost.Core/           # Core Domain, Models, and Services (No UI dependencies)
│   │   ├── Models/
│   │   │   ├── WiFiInterfaceStatus.cs  # Network interface states
│   │   │   ├── BSSIDNetwork.cs         # Scanned AP properties & score
│   │   │   ├── AppSettings.cs          # User settings & auto-clean preferences
│   │   │   ├── ProcessMemoryItem.cs    # [NEW] Process info, RAM MB, and extracted Icon
│   │   │   ├── InstalledAppItem.cs     # [NEW] Installed software info, disk usage size, and icon
│   │   │   ├── SystemMemoryStatus.cs   # [NEW] Total, Used, Free RAM gauge
│   │   │   └── JunkScanResult.cs       # [NEW] Safe junk file counts and sizes
│   │   ├── Services/
│   │   │   ├── IWiFiService.cs / NativeWifiService.cs
│   │   │   ├── AutoSwitchEngine.cs
│   │   │   ├── ISpeedTestService.cs / SpeedTestService.cs
│   │   │   ├── ISettingsService.cs / SettingsService.cs
│   │   │   ├── LocalizationService.cs  # [UPDATED] Bilingual strings for RAM, Junk Cleaner & Uninstaller
│   │   │   ├── ISystemOptimizerService.cs / SystemOptimizerService.cs # RAM & Junk Engine
│   │   │   └── ISoftwareUninstallerService.cs / SoftwareUninstallerService.cs # [NEW] Uninstaller & App Manager
│   │   └── ViewModels/
│   │       ├── ViewModelBase.cs
│   │       ├── RelayCommand.cs
│   │       └── MainViewModel.cs        # [UPDATED] Coordinates Wi-Fi, SpeedTest, Optimizer, and Uninstaller tabs
│   └── HyperBoost.App/            # WPF UI Layer
│       ├── Resources/
│       │   ├── app.ico                 # Custom HyperBoost Cybernetic Icon
│       │   └── Themes/                 # ResourceDictionaries (HyperDark, CyberNeon, OLEDBlack)
│       ├── Converters/                 # WPF Value Converters (IconToImageSource, etc.)
│       ├── App.xaml / App.xaml.cs      # System Tray & Startup logic
│       └── MainWindow.xaml             # [UPDATED] 4-Tab Layout with Log Filtering and Uninstaller DataGrid
├── tests/
│   └── HyperBoost.Tests/          # Automated TDD Test Suite (xUnit)
│       ├── BSSIDNetworkTests.cs
│       ├── AutoSwitchEngineTests.cs
│       ├── PackagingAndGitHubTests.cs
│       └── SystemOptimizerTests.cs     # TDD tests for RAM cleaner, Process list, and Safe junk scan
├── release_v2.2/                       # Final self-contained single-file portable executable
├── Architecture.md                     # This document
├── AI_Learnings.md                     # Knowledge base and technical discoveries
├── README.md                           # Viral open-source promotional documentation
└── LICENSE                             # MIT License
```

---

## 3. Technical Specifications & Native Interop
### 3.1 Win32 Memory & Process Optimization (`psapi.dll` & `kernel32.dll`)
- `GlobalMemoryStatusEx`: Retreives accurate 64-bit physical memory statistics aligned with Windows Task Manager calculation.
- `EmptyWorkingSet`: Trims idle RAM pages across accessible processes.
- `SHGetFileInfo`: Extracts high-resolution associated icons (`HICON`) from process executables and converts them to WPF `BitmapSource` for seamless UI rendering.

### 3.2 Safe Junk Cleaner Protocol
- **Recycle Bin**: Uses `SHEmptyRecycleBin` Win32 API with flags `SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND`.
- **Temp Files**: Recursively traverses `%TEMP%` and `C:\Windows\Temp` subdirectories using safe enumeration, deleting files with `LastAccessTimeUtc < DateTime.UtcNow.AddHours(-24)` while gracefully handling file-lock IOExceptions.

---

## 4. Implementation Roadmap (TDD Workflow)
- [x] **Phase 1: Foundation & Wi-Fi Steering** (Completed - 27/27 tests passing)
- [x] **Phase 2: Rebranding & Custom Icon Generation** (HyperBoost 5G & PC Suite)
- [x] **Phase 3: TDD & Core Engine for System Optimizer (`SystemOptimizerService`)**
  - [x] Create `SystemOptimizerTests.cs` (TDD First).
  - [x] Implement `SystemOptimizerService` (RAM gauge aligned with Task Manager, Process list with icons, End Task, Auto-clean timer, Recursive safe junk scanner).
- [x] **Phase 4: UI/UX Optimization & Tab Consolidation (`MainWindow.xaml` & `MainViewModel`)**
  - [x] Consolidated Speed Test directly into Main Dashboard (Tab 0) side-by-side with Wi-Fi tables.
  - [x] Restored glowing Wi-Fi Signal Progress Bars in network grids and upgraded to **Segmented Wi-Fi Signal Gauge** (`SegmentedSignalBarStyle`) using `VisualBrush` & `UniformGrid` mask for multi-chunk LED styling.
  - [x] Full Responsive Layout: Replaced fixed pixel widths in all DataGrids and containers with proportional Star-Sizing (`*`), enabled auto horizontal/vertical scrollbars, and added text truncation/wrapping to prevent any UI clipping when resizing or snapping window.
- [x] **Phase 5: Final Packaging & CI/CD Release (v2.0.0)**

