using System.Globalization;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

/// <summary>
///     A value converter that converts between integers and strings
///     Used in XAML data binding to convert integers to strings and vice versa
/// </summary>
public class IntToStringConverter : IValueConverter
{
    /// <summary>
    ///     Converts an integer value to its string representation
    /// </summary>
    /// <param name="value">The integer value to convert</param>
    /// <param name="targetType">The target type (not used in this implementation)</param>
    /// <param name="parameter">Converter parameter (not used in this implementation)</param>
    /// <param name="culture">Culture info for formatting</param>
    /// <returns>The string representation of the integer value, or "30" as default</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int intValue ? intValue.ToString(CultureInfo.InvariantCulture) : "30";
    }

    /// <summary>
    ///     Converts a string value back to an integer
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <param name="targetType">The target type (not used in this implementation)</param>
    /// <param name="parameter">Converter parameter (not used in this implementation)</param>
    /// <param name="culture">Culture info (not used in this implementation)</param>
    /// <returns>The parsed integer value, or 30 as default if parsing fails</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string stringValue && int.TryParse(stringValue, out var result) ? result : 30;
    }
}