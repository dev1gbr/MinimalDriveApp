using System.Globalization;
using System.Windows.Data;

namespace MinimalDriveApp.Views;

[ValueConversion(typeof(long), typeof(string))]
public class BytesToGbConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is long bytes ? $"{bytes / 1_073_741_824.0:F1} GB" : "—";

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
