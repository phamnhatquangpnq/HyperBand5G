// Standardized to production level
// Purpose: WPF Value Converter to extract Windows associated icon from executable path and return cached BitmapSource
// Dependencies: System, System.Collections.Concurrent, System.Drawing, System.Globalization, System.Windows, System.Windows.Data, System.Windows.Interop, System.Windows.Media, System.Windows.Media.Imaging

namespace HyperBoost.App.Converters;

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ExePathToIconConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, ImageSource?> _iconCache = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string exePath || string.IsNullOrWhiteSpace(exePath))
            return null;

        return _iconCache.GetOrAdd(exePath, path =>
        {
            try
            {
                if (File.Exists(path))
                {
                    using var icon = Icon.ExtractAssociatedIcon(path);
                    if (icon != null)
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze(); // Mandatory for cross-thread MVVM binding
                        return bitmapSource;
                    }
                }
            }
            catch
            {
                // Access denied or invalid exe format
            }
            return null;
        });
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
