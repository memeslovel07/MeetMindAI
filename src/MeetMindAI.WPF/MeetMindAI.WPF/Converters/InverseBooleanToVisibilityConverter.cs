using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MeetMindAI.WPF.Converters;

public sealed class InverseBooleanToVisibilityConverter
    : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return value is bool isVisible && !isVisible
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
