using System.Windows;
using System.Windows.Media;

namespace SnowFlake.Utils;

public abstract class DpiUtil
{
    public static (double dpiX, double dpiY) GetDpi(Window window)
    {
        var source = PresentationSource.FromVisual(window);

        if (source?.CompositionTarget == null)
            return (96, 96); // 默认DPI

        var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
        var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

        return (dpiX, dpiY);
    }
    
    // 允许你传递任何Visual对象而不仅仅是Window对象
    public static (double dpiX, double dpiY) GetDpi(Visual visual)
    {
        var source = PresentationSource.FromVisual(visual);

        if (source?.CompositionTarget == null)
            return (96, 96); // 默认DPI

        var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
        var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

        return (dpiX, dpiY);
    }
}