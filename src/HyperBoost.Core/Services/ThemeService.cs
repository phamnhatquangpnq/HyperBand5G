// Standardized to production level
// Purpose: Theme color provider mapping theme names to hex colors for real-time WPF resource updates
// Dependencies: System

namespace HyperBoost.Core.Services;

public record ThemeColors(string Background, string Panel, string Border, string Accent, string Green, string Orange, string Red, string Text, string Muted);

public static class ThemeService
{
    public static ThemeColors GetThemeColors(string themeName)
    {
        return themeName switch
        {
            "CyberNeon" => new ThemeColors("#0B0813", "#161024", "#2C1F4A", "#FF007F", "#00FFCC", "#FF9900", "#FF3355", "#F8FAFC", "#94A3B8"),
            "OLEDBlack" => new ThemeColors("#000000", "#0A0A0A", "#1A1A1A", "#38BDF8", "#22C55E", "#F97316", "#EF4444", "#FFFFFF", "#6B7280"),
            "MatrixEmerald" => new ThemeColors("#05130E", "#0A241A", "#144531", "#00FF66", "#00F280", "#F59E0B", "#EF4444", "#ECFDF5", "#6EE7B7"),
            "SunsetPulse" => new ThemeColors("#11081A", "#1E102E", "#3B1F5C", "#FF5E36", "#34D399", "#FBBF24", "#F43F5E", "#FDF4FF", "#C084FC"),
            "NordicFrost" => new ThemeColors("#0F172A", "#1E293B", "#334155", "#38BDF8", "#2DD4BF", "#FBA918", "#F87171", "#F8FAFC", "#94A3B8"),
            "GoldPrestige" => new ThemeColors("#0A0A0C", "#141418", "#2A2A35", "#F59E0B", "#10B981", "#FBBF24", "#EF4444", "#FFFBEB", "#D97706"),
            _ => new ThemeColors("#0A0D14", "#121722", "#1F293D", "#00D2FF", "#00F280", "#FF9900", "#FF3355", "#E2E8F0", "#64748B") // HyperDark Default
        };
    }
}
