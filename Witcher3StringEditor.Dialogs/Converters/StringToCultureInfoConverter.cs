using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

internal class StringToCultureInfoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string ci) return !string.IsNullOrWhiteSpace(ci) ? new CultureInfo(ci) : new CultureInfo("en");

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is CultureInfo ci ? ci.Name : DependencyProperty.UnsetValue;
    }
}