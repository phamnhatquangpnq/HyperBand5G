# HyperBand 5G Suite - Smart Wi-Fi Steering & Speed Test Pro

[![Build Status](https://img.shields.io/badge/build-passing-00F280?style=for-the-badge&logo=dotnet)](https://github.com/)
[![Tests](https://img.shields.io/badge/tests-18%20passed-00F280?style=for-the-badge)](https://github.com/)
[![License](https://img.shields.io/badge/license-MIT-00D2FF?style=for-the-badge)](https://github.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-00D2FF?style=for-the-badge&logo=windows)](https://github.com/)
[![Framework](https://img.shields.io/badge/.NET-10.0%20WPF-68217A?style=for-the-badge&logo=dotnet)](https://github.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=for-the-badge)](https://github.com/)

---

### ⚡ Stop Windows from Bouncing Between 2.4 GHz and 5 GHz!
When you connect your laptop to a combined/mesh Wi-Fi network (where 2.4 GHz and 5 GHz share the exact same Wi-Fi name / SSID), Windows wireless drivers notoriously default to the slower, congested 2.4 GHz band or constantly bounce back and forth during gaming and downloads.

**HyperBand 5G Suite** solves this problem permanently. It runs silently in your Windows System Tray, actively monitors your wireless connection in real-time, and **instantly forces your adapter to lock onto the faster 5 GHz (or 6 GHz) access point** the second Windows drops you to 2.4 GHz!

---

## 🔥 Key Features

1. **🤖 Autonomous 5GHz Band Steering & Auto-Lock**:
   - Actively monitors your wireless adapter (configurable polling interval from 1s to 10s).
   - Detects real-time band drops and executes instant BSSID steering to lock onto the fastest 5 GHz / 6 GHz access point broadcasted by your router.
2. **🚀 Built-In Wi-Fi Speed Test (Powered by Cloudflare CDN)**:
   - Integrated real-time network diagnostic suite powered by Cloudflare's global edge infrastructure (`speed.cloudflare.com`).
   - Measures live **Latency (Ping)**, **Jitter**, **Download Speed (Mbps)**, and **Upload Speed (Mbps)** with glowing neon progress bars and real-time meters.
3. **🎨 Real-Time Dynamic Themes (Zero Restart Required)**:
   - Switch instantly between **HyperDark Space (Default)**, **CyberNeon Glow (Cyberpunk)**, and **OLED Deep Black (Stealth)**.
   - Built on WPF `{DynamicResource}` architecture so theme changes apply immediately across all open windows.
4. **🌐 Multi-Language Support (🇻🇳 Tiếng Việt & 🇺🇸 English)**:
   - Toggle languages in real-time with flag buttons on the dashboard.
   - Fully localized UI titles, table headers, descriptions, speed test meters, and Windows Desktop Toast Notifications.
5. **📊 Smart Dashboard & Network Audit**:
   - **Authorized Networks Table**: Lists all BSSIDs broadcasted by your current connected SSID. The **currently connected Wi-Fi row is highlighted in bold** with a glowing green **★ ĐANG DÙNG / ACTIVE** badge.
   - **Available Networks Table**: Displays surrounding Wi-Fi networks with signal scores and hardware standards.
6. **⚙️ Windows Startup Integration & System Tray Mode**:
   - Configure automatic background launch on Windows Startup via Registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
   - Minimizes silently to the System Tray with balloon notifications whenever an automatic band switch occurs.
7. **💎 Standalone Single-File Executable (Zero Dependencies)**:
   - Packaged as a **Self-Contained Single-File Executable (`HyperBand5G.exe`)**. You don't even need .NET installed on your PC to run it!

---

## 📥 Download & Installation (Standalone Executable)

You don't need to install any frameworks or dependencies. Just download and run!

1. Go to the **[Releases Page](../../releases/latest)** of this repository.
2. Download the latest single-file executable: **`HyperBand5G.exe`**.
3. Double-click **`HyperBand5G.exe`** to launch the suite!
   - *Tip: Right-click the system tray icon to open the dashboard or toggle Smart 5GHz Lock quickly.*

---

## 🛠️ Building From Source

If you are a developer and want to build or modify the codebase:

### Prerequisites
- **Windows 10 or Windows 11** (64-bit).
- **.NET 10.0 SDK** (or .NET 8/9 SDK with compatible target framework).

### Commands
1. **Clone the repository**:
   ```powershell
   git clone https://github.com/yourusername/HyperBand5G.git
   cd HyperBand5G
   ```
2. **Run the TDD Test Suite** (18/18 Automated Tests):
   ```powershell
   dotnet test
   ```
3. **Build & Run Locally**:
   ```powershell
   dotnet run --project src/WifiBandLockPro.App/WifiBandLockPro.App.csproj
   ```
4. **Publish Standalone Executable**:
   ```powershell
   dotnet publish src/WifiBandLockPro.App/WifiBandLockPro.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./release
   ```

---

## 🏗️ Technical Architecture & TDD

Built autonomously with High-Precision Architecture and strict **Test-Driven Development (TDD)** principles:

```
HyperBand5G/
├── src/
│   ├── WifiBandLockPro.Core/               # Core Domain & Logic (.NET 10 Library)
│   │   ├── Models/                         # Immutable Records (WiFiBand, BSSIDNetwork, SpeedTestStatus)
│   │   ├── Services/                       # Netsh wlan parser, AutoSwitchEngine, Cloudflare Speed Test, ThemeService
│   │   └── ViewModels/                     # MVVM MainViewModel & RelayCommand
│   └── WifiBandLockPro.App/                # Presentation Layer (.NET 10 WPF Application)
│       ├── app.ico                         # Custom Cyberpunk Neon Wi-Fi Radar Icon
│       ├── App.xaml / App.xaml.cs          # System Tray NotifyIcon & Dynamic Theme Switcher
│       └── MainWindow.xaml / .xaml.cs      # HyperBand 5G Dashboard & Speed Test UI
└── tests/
    └── WifiBandLockPro.Tests/              # TDD Test Suite (xUnit - 18 Tests)
```

---

## 🤝 Contributing & License
Contributions, issues, and feature requests are welcome! Feel free to check the issues page.
Distributed under the **MIT License**. See `LICENSE` for more information.

---
*⭐ Star this repository if it saved your Wi-Fi connection from dropping to 2.4 GHz!*
