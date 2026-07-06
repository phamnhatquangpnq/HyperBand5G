// Standardized to production level
// Purpose: Bilingual localization service (Vietnamese & English) for UI text binding
// Dependencies: System, System.ComponentModel

namespace HyperBoost.Core.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class LocalizationService : INotifyPropertyChanged
{
    private string _currentLanguage = "vn";

    public LocalizationService(string defaultLanguage = "vn")
    {
        _currentLanguage = defaultLanguage.ToLower() == "en" ? "en" : "vn";
    }

    public void SetLanguage(string lang)
    {
        if (_currentLanguage != lang)
        {
            _currentLanguage = lang.ToLower() == "en" ? "en" : "vn";
            OnPropertyChanged(string.Empty); // Notify all properties changed
        }
    }

    public bool IsVietnamese => _currentLanguage == "vn";

    // --- Header & Subtitle ---
    public string AppTitle => IsVietnamese ? "⚡ HYPERBOOST 5G & PC SUITE v2.0" : "⚡ HYPERBOOST 5G & PC SUITE v2.0";
    public string AppSubtitle => IsVietnamese 
        ? "Tự Động Khóa Băng Tần 5GHz/6GHz • Tối Ưu RAM & Dọn Rác Máy Tính An Toàn" 
        : "Autonomous 5GHz/6GHz Wi-Fi Steering • Smart RAM Booster & Safe PC Optimizer";

    // --- Tabs ---
    public string DashboardTabTitle => IsVietnamese ? "📡 QUẢN LÝ WI-FI & ĐO TỐC ĐỘ" : "📡 WI-FI & SPEED TEST";
    public string SpeedTestTabTitle => IsVietnamese ? "🚀 ĐO TỐC ĐỘ WI-FI" : "🚀 WI-FI SPEED TEST";
    public string SystemOptimizerTabTitle => IsVietnamese ? "⚡ TỐI ƯU RAM & DỌN RÁC" : "⚡ RAM & JUNK OPTIMIZER";
    public string UninstallerTabTitle => IsVietnamese ? "🗑️ GỠ CÀI ĐẶT PHẦN MỀM" : "🗑️ APP UNINSTALLER";

    // --- Uninstaller Section ---
    public string UninstallerTitle => IsVietnamese ? "QUẢN LÝ & GỠ CÀI ĐẶT PHẦN MỀM (APP UNINSTALLER)" : "INSTALLED SOFTWARE & APP UNINSTALLER";
    public string UninstallerDesc => IsVietnamese ? "Danh sách toàn bộ phần mềm đã cài đặt trên máy tính. Hỗ trợ gỡ cài đặt sạch sẽ, xem chi tiết và mở thư mục." : "List of all installed software on your PC. Supports clean uninstall, detailed info, and open location.";
    public string ColAppName => IsVietnamese ? "Tên Ứng Dụng" : "Application Name";
    public string ColAppVersion => IsVietnamese ? "Phiên Bản" : "Version";
    public string ColAppPublisher => IsVietnamese ? "Nhà Phát Hành" : "Publisher";
    public string ColAppDate => IsVietnamese ? "Ngày Cài" : "Install Date";
    public string ColAppSize => IsVietnamese ? "Dung Lượng" : "Size";
    public string BtnUninstall => IsVietnamese ? "🗑️ Gỡ Cài Đặt" : "🗑️ Uninstall";
    public string BtnAppInfo => IsVietnamese ? "ℹ️ Thông Tin" : "ℹ️ Info";
    public string BtnOpenFolder => IsVietnamese ? "📂 Mở Thư Mục" : "📂 Open Folder";

    // --- Dashboard Section ---
    public string ActiveNetworkTitle => IsVietnamese ? "KẾT NỐI WI-FI HIỆN TẠI (ACTIVE CONNECTION)" : "CURRENT ACTIVE WI-FI CONNECTION";
    public string AvailableNetworksTitle => IsVietnamese ? "CÁC BĂNG TẦN BSSID KHÁC TRONG PHẠM VI (AVAILABLE BSSIDs)" : "OTHER NEARBY BSSIDs & WIRELESS BANDS";
    public string SmartLockStatusLabel => IsVietnamese ? "Chế Độ Khóa 5GHz:" : "5GHz Auto-Lock Status:";

    // --- Speed Test Section ---
    public string SpeedTestHeader => IsVietnamese ? "KIỂM TRA TỐC ĐỘ MẠNG THỰC TẾ (CLOUDFLARE EDGE CDN)" : "REAL-TIME EDGE NETWORK SPEED TEST (CLOUDFLARE CDN)";
    public string PingLabel => IsVietnamese ? "Độ Trễ (Ping):" : "Latency (Ping):";
    public string JitterLabel => IsVietnamese ? "Độ Dao Động (Jitter):" : "Jitter (Variation):";
    public string DownloadLabel => IsVietnamese ? "Tốc Độ Tải Xuống (Download):" : "Download Bandwidth:";
    public string UploadLabel => IsVietnamese ? "Tốc Độ Tải Lên (Upload):" : "Upload Bandwidth:";
    public string BtnStartSpeedTest => IsVietnamese ? "⚡ BẮT ĐẦU ĐO TỐC ĐỘ" : "⚡ START SPEED TEST";
    public string BtnStopSpeedTest => IsVietnamese ? "🛑 DỪNG ĐO" : "🛑 ABORT TEST";

    // --- System Optimizer Section ---
    public string RAMHeader => IsVietnamese ? "QUẢN LÝ BỘ NHỚ RAM & TỐI ƯU HỆ THỐNG" : "SYSTEM RAM MONITOR & MEMORY BOOSTER";
    public string TotalRAMLabel => IsVietnamese ? "Tổng RAM Hệ Thống:" : "Total System RAM:";
    public string UsedRAMLabel => IsVietnamese ? "Đang Sử Dụng:" : "Currently Used:";
    public string FreeRAMLabel => IsVietnamese ? "Còn Trống (Available):" : "Available Memory:";
    public string BtnOptimizeRAM => IsVietnamese ? "⚡ TỐI ƯU RAM NGAY" : "⚡ OPTIMIZE RAM NOW";
    public string BtnEndTask => IsVietnamese ? "❌ ĐÓNG ỨNG DỤNG" : "❌ END TASK";
    public string ProcessListTitle => IsVietnamese ? "DANH SÁCH ỨNG DỤNG ĐANG CHẠY (TOP MEMORY CONSUMERS)" : "RUNNING PROCESSES (TOP MEMORY CONSUMERS)";
    public string ColProcIcon => IsVietnamese ? "Icon" : "Icon";
    public string ColProcName => IsVietnamese ? "Tên Ứng Dụng" : "Application Name";
    public string ColProcRAM => IsVietnamese ? "RAM Sử Dụng" : "Memory (MB)";
    public string ColProcAction => IsVietnamese ? "Thao Tác" : "Action";
    public string AutoCleanRAMLabel => IsVietnamese ? "Tự động dọn RAM (giây):" : "Auto-clean RAM (seconds):";

    // --- Junk Cleaner Section ---
    public string JunkHeader => IsVietnamese ? "DỌN RÁC HỆ THỐNG AN TOÀN (SAFE JUNK CLEANER)" : "SAFE SYSTEM JUNK CLEANER (NON-DESTRUCTIVE)";
    public string JunkDesc => IsVietnamese 
        ? "Quy tắc an toàn: Chỉ dọn file tạm %TEMP%, Windows\\Temp, C:\\Temp và Thùng rác. Tuyệt đối không xóa file cá nhân hay hệ thống."
        : "Safety Protocol: Strictly cleans %TEMP%, Windows\\Temp, C:\\Temp, and Recycle Bin. Never touches personal or system files.";
    public string BtnScanJunk => IsVietnamese ? "🔍 QUÉT RÁC" : "🔍 SCAN JUNK";
    public string BtnCleanJunk => IsVietnamese ? "🧹 DỌN SẠCH AN TOÀN" : "🧹 CLEAN SAFE JUNK";
    public string JunkUserTempLabel => IsVietnamese ? "• File rác tạm thời người dùng (%TEMP%):" : "• User Temporary Files (%TEMP%):";
    public string JunkWinTempLabel => IsVietnamese ? "• File rác tạm thời hệ thống (Windows\\Temp & C:\\Temp):" : "• System Temp Files (Windows\\Temp & C:\\Temp):";
    public string JunkRecycleBinLabel => IsVietnamese ? "• Thùng rác hệ thống (Recycle Bin):" : "• Windows Recycle Bin Items:";
    public string JunkLogsLabel => IsVietnamese ? "• File nhật ký & báo cáo lỗi cũ (Crash Logs):" : "• System Error Reporting & Crash Logs:";
    public string TotalJunkFoundLabel => IsVietnamese ? "TỔNG DUNG LƯỢNG RÁC TÌM THẤY:" : "TOTAL JUNK SIZE FOUND:";

    // --- Common Tables & Settings ---
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
    public string BadgeActive => IsVietnamese ? "★ ĐANG DÙNG WIFI NÀY" : "★ CURRENTLY USING";

    // --- UI Alias & Missing Binding Fixes ---
    public string OptimizerTabTitle => SystemOptimizerTabTitle;
    public string SettingsTabTitle => IsVietnamese ? "⚙️ CÀI ĐẶT" : "⚙️ SETTINGS";
    public string SmartSelectionTitle => IsVietnamese ? "TỰ ĐỘNG KHÓA BĂNG TẦN 5GHZ/6GHZ (SMART STEERING)" : "5GHZ/6GHZ SMART BAND STEERING & LOCK";
    public string ActiveMonitoring => IsVietnamese ? "★ ĐANG GIÁM SÁT NGẦM" : "★ ACTIVE MONITORING";
    public string SmartSelectionDesc => IsVietnamese ? "Tự động phát hiện khi router chuyển sang 2.4GHz chậm chạp và lập tức ép kết nối lại BSSID 5GHz/6GHz tốc độ cao nhất." : "Automatically detects drops to slow 2.4GHz bands and instantly switches your Wi-Fi card back to the fastest 5GHz/6GHz BSSID.";
    public string AuthorizedNetworksTitle => IsVietnamese ? "DANH SÁCH BSSID HIỆN TẠI ĐANG KẾT NỐI & TỐI ƯU" : "CURRENT CONNECTED & OPTIMIZED BSSID LIST";
    public string AuthorizedNetworksDesc => IsVietnamese ? "Các điểm truy cập (BSSID) thuộc cùng tên mạng Wi-Fi của bạn." : "Access points (BSSIDs) belonging to your current Wi-Fi network.";
    public string SpeedTestDesc => IsVietnamese ? "Đo tốc độ thực tế với máy chủ edge Cloudflare toàn cầu (Độ trễ, Jitter, Download, Upload)." : "Measure real-world edge performance with global Cloudflare servers (Ping, Jitter, Download, Upload).";
    public string RamBoosterTitle => RAMHeader;
    public string RamBoosterDesc => IsVietnamese ? "Trích xuất icon ứng dụng, hiển thị RAM thực tế và dọn dẹp bộ nhớ đệm Standby List không gây lỗi app." : "Extracts app icons, monitors real-time RAM, and trims Standby List memory without crashing apps.";
    public string BtnOptimizeRam => BtnOptimizeRAM;
    public string ColProcId => "ID";
    public string ColProcRam => ColProcRAM;
    public string JunkCleanerTitle => JunkHeader;
    public string JunkCleanerDesc => JunkDesc;
    public string AutoCleanRamLabel => AutoCleanRAMLabel;
    public string ActivityLogTitle => IsVietnamese ? "📜 NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG" : "📜 SYSTEM ACTIVITY LOGS";
    public string ActivityLogDesc => IsVietnamese ? "Ghi nhận mọi thao tác tự động chuyển băng tần và dọn RAM." : "Records all automatic band steering and RAM optimization events.";
    public string BtnClearLogs => IsVietnamese ? "🗑️ Xóa Nhật Ký" : "🗑️ Clear Logs";
    public string BtnUltraCompress => IsVietnamese ? "⚡ Ultra COMPRESS Mode - Siêu Tiết Kiệm Ổ C:" : "⚡ Ultra COMPRESS Mode - Super Space Save C:";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
