// Standardized to production level
// Purpose: MVVM Localization Service supporting real-time language switching between Vietnamese (VN) and English (EN)
// Dependencies: System.ComponentModel, System.Runtime.CompilerServices

namespace WifiBandLockPro.Core.Services;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class LocalizationService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentLang = "vn"; // Default Vietnamese

    public string CurrentLang => _currentLang;
    public bool IsVietnamese => _currentLang == "vn";

    public LocalizationService(string initialLang = "vn")
    {
        _currentLang = string.Equals(initialLang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vn";
    }

    public void SetLanguage(string lang)
    {
        if (!string.Equals(_currentLang, lang, StringComparison.OrdinalIgnoreCase))
        {
            _currentLang = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vn";
            // Fire PropertyChanged for ALL properties by passing null/empty string
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    // --- Localized Strings (Rebranded to HyperBand 5G) ---

    public string AppSubtitle => IsVietnamese ? "HYPERBAND 5G SUITE" : "HYPERBAND 5G SUITE";
    public string DashboardTabTitle => IsVietnamese ? "★ TRUNG TÂM ĐIỀU KHIỂN" : "★ INTELLIGENCE CENTER";
    public string SpeedTestTabTitle => IsVietnamese ? "🚀 ĐO TỐC ĐỘ WI-FI" : "🚀 WI-FI SPEED TEST";
    public string SettingsTabTitle => IsVietnamese ? "⚙ CÀI ĐẶT HỆ THỐNG" : "⚙ SETTINGS";

    public string SmartSelectionTitle => IsVietnamese ? "Tự Động Chọn Điểm Phát Sóng (Khóa 5GHz)" : "Smart Access Point Selection (5GHz Auto-Lock)";
    public string SmartSelectionDesc => IsVietnamese 
        ? "Tự động phát hiện khi Windows bị rớt xuống băng tần 2.4 GHz trên Wi-Fi gộp và ngay lập tức ép chuyển lại băng tần 5 GHz tốc độ cao." 
        : "Automatically detects when Windows switches to 2.4 GHz on dual-band SSIDs and instantly forces a switch back to the faster 5 GHz band.";
    public string ActiveMonitoring => IsVietnamese ? "ĐANG GIÁM SÁT" : "ACTIVE MONITORING";

    public string AuthorizedNetworksTitle => IsVietnamese ? "Mạng Đang Kết Nối (Các điểm phát BSSID của SSID hiện tại)" : "Authorized Networks (Current SSID BSSIDs)";
    public string AuthorizedNetworksDesc => IsVietnamese ? "- Điểm phát sóng từ router bạn đang kết nối" : "- Access points broadcasted by your active router";
    public string AvailableNetworksTitle => IsVietnamese ? "Các Mạng Khả Dụng (Wi-Fi xung quanh)" : "Available Networks (Other Nearby Wi-Fi)";
    
    public string ActivityLogTitle => IsVietnamese ? "Lịch Sử Tự Động Chuyển Băng Tần & Nhật Ký Hệ Thống" : "Auto-Switch Activity Log & Band Steering Audit";
    public string ActivityLogDesc => IsVietnamese ? "- Chi tiết các sự kiện chuyển băng tần tự động theo thời gian thực" : "- Real-time events when Windows switches bands";

    public string SpeedTestDesc => IsVietnamese ? "- Kiểm tra tốc độ thực tế Ping, Jitter, Download & Upload qua CDN Cloudflare" : "- Real-time Ping, Jitter, Download & Upload test via Cloudflare CDN";
    public string BtnStartSpeedTest => IsVietnamese ? "BẮT ĐẦU ĐO TỐC ĐỘ" : "START SPEED TEST";
    public string PingLabel => IsVietnamese ? "Độ Trễ (Ping)" : "Latency (Ping)";
    public string JitterLabel => IsVietnamese ? "Độ Nhiễu (Jitter)" : "Jitter";
    public string DownloadLabel => IsVietnamese ? "Tốc Độ Tải Xuống (Download)" : "Download Speed";
    public string UploadLabel => IsVietnamese ? "Tốc Độ Tải Lên (Upload)" : "Upload Speed";

    public string ColName => IsVietnamese ? "Tên Wi-Fi" : "Name";
    public string ColBssid => IsVietnamese ? "Mã BSSID" : "Unique ID (BSSID)";
    public string ColScore => IsVietnamese ? "Điểm số" : "Score";
    public string ColSignal => IsVietnamese ? "Tín hiệu" : "Signal Strength";
    public string ColBand => IsVietnamese ? "Băng tần" : "Band";
    public string ColClass => IsVietnamese ? "Chuẩn" : "Class";
    public string ColAction => IsVietnamese ? "Thao tác" : "Action";
    public string ColStatus => IsVietnamese ? "Trạng thái" : "Status";

    public string BtnForceConnect => IsVietnamese ? "Ép kết nối" : "Force Connect";
    public string BtnRefresh => IsVietnamese ? "Quét Mạng" : "Refresh Scan";
    public string BtnSettings => IsVietnamese ? "Cài Đặt" : "Settings";

    public string SettingsTitle => IsVietnamese ? "Cài Đặt Hệ Thống & Tùy Chỉnh" : "Settings & Preferences";
    public string StartupLabel => IsVietnamese ? "Khởi động cùng Windows (Chạy ngầm trong System Tray)" : "Run on Windows Startup (Silent in System Tray)";
    public string ThemeLabel => IsVietnamese ? "Giao diện (Theme):" : "Color Theme:";
    public string PollIntervalLabel => IsVietnamese ? "Tần suất kiểm tra (giây):" : "Polling Interval (seconds):";
    public string ThresholdLabel => IsVietnamese ? "Ngưỡng tín hiệu 5GHz tối thiểu (%):" : "Min 5GHz Signal Threshold (%):";
    public string LanguageLabel => IsVietnamese ? "Ngôn ngữ / Language:" : "Language / Ngôn ngữ:";
    public string BtnClose => IsVietnamese ? "Đóng" : "Close";
    public string BtnSaveSettings => IsVietnamese ? "Lưu Cài Đặt" : "Save Settings";

    public string Badge5Ghz => IsVietnamese ? "5 GHz (Tối ưu)" : "5 GHz (Active)";
    public string Badge24Ghz => IsVietnamese ? "2.4 GHz (Cảnh báo - Chậm)" : "2.4 GHz (Warning - Slow Band)";
    public string BadgeScanning => IsVietnamese ? "Đang quét..." : "Scanning...";
    public string BadgeDisconnected => IsVietnamese ? "Mất kết nối" : "Disconnected";
    public string BadgeActive => IsVietnamese ? "★ ĐANG DÙNG" : "★ ACTIVE";
}
