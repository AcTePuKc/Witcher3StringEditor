using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

internal class StringToCultureInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string ci)
        {
            if (!string.IsNullOrWhiteSpace(ci))
            {
                return new CultureInfo(ci);
            }
            else
            {
                return new CultureInfo("en");
            }
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is CultureInfo ci ? ci.Name : DependencyProperty.UnsetValue;
    }
}