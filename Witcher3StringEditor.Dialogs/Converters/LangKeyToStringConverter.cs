using System.Globalization;
using System.Windows.Data;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.Converters;

/// <summary>
///     A value converter that translates language keys to their corresponding localized strings.
/// </summary>
public class LangKeyToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return Strings.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
