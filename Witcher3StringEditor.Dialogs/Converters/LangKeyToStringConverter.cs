using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

/// <summary>
///     A value converter that translates language keys to their corresponding localized strings
///     Used in XAML data binding to convert resource keys to actual display strings based on the current culture
/// </summary>
public class LangKeyToStringConverter : IValueConverter
{
    /// <summary>
    ///     Converts a language key to its corresponding localized string
    /// </summary>
    /// <param name="value">The language key to convert (expected to be a string)</param>
    /// <param name="targetType">The type of the binding target property (not used in this implementation)</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic (not used in this implementation)</param>
    /// <param name="culture">The culture to use in the converter (not used in this implementation)</param>
    /// <returns>The localized string corresponding to the key, or DependencyProperty.UnsetValue if conversion fails</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return value is not string key ? DependencyProperty.UnsetValue : I18NExtension.Translate(key);
        }
        catch (Exception)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    ///     Converts a value back to a language key (not implemented)
    /// </summary>
    /// <param name="value">The value to convert back</param>
    /// <param name="targetType">The type to convert to</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic</param>
    /// <param name="culture">The culture to use in the converter</param>
    /// <returns>Throws NotImplementedException as this operation is not supported</returns>
    /// <exception cref="NotImplementedException">This method is not implemented</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}