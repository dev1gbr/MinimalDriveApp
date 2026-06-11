using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MinimalDriveApp.Views;

[ValueConversion(typeof(long), typeof(string))]
public class BytesToGbConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is long bytes ? (bytes / 1_073_741_824.0).ToString("F1", CultureInfo.InvariantCulture) + " GB" : "—";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

[ValueConversion(typeof(DateTime?), typeof(string))]
public class NullableDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is DateTime dt ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "—";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Multi-value converter: [usedBytes (long), totalBytes (long)] → PathGeometry for the used arc.
/// Renders a donut-chart arc from the top (–90°) sweeping clockwise.
/// </summary>
public class DonutArcConverter : IMultiValueConverter
{
    private const double DefaultOuter = 80;
    private const double DefaultInner = 52;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Geometry.Empty;

        double used  = System.Convert.ToDouble(values[0]);
        double total = System.Convert.ToDouble(values[1]);
        if (total <= 0) return Geometry.Empty;

        double outerR = DefaultOuter;
        double innerR = DefaultInner;
        if (parameter is string p)
        {
            var parts = p.Split(',');
            if (parts.Length == 2)
            {
                double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out outerR);
                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out innerR);
            }
        }

        double fraction = Math.Clamp(used / total, 0, 1);
        if (fraction >= 0.9999) fraction = 0.9999;
        if (fraction <= 0.0001) return Geometry.Empty;

        double cx = outerR;
        double cy = outerR;
        double sweepDeg = fraction * 360;
        double startRad = -Math.PI / 2;
        double endRad   = startRad + sweepDeg * Math.PI / 180;

        Point outerStart = Pt(cx, cy, outerR, startRad);
        Point outerEnd   = Pt(cx, cy, outerR, endRad);
        Point innerEnd   = Pt(cx, cy, innerR, endRad);
        Point innerStart = Pt(cx, cy, innerR, startRad);
        bool  large      = sweepDeg > 180;

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(outerStart, isFilled: true, isClosed: true);
            ctx.ArcTo(outerEnd, new Size(outerR, outerR), 0, large, SweepDirection.Clockwise,         true, false);
            ctx.LineTo(innerEnd, true, false);
            ctx.ArcTo(innerStart, new Size(innerR, innerR), 0, large, SweepDirection.Counterclockwise, true, false);
        }
        geo.Freeze();
        return geo;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Point Pt(double cx, double cy, double r, double rad)
        => new(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
}

/// <summary>
/// Converts bool to Visibility. Pass parameter="Invert" to flip the logic.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
