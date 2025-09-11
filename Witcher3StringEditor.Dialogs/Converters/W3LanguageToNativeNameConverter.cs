using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Dialogs.Converters;

public class W3LanguageToNativeNameConverter:IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if(value is not W3Language language) return DependencyProperty.UnsetValue;
      return new CultureInfo(typeof(W3Language).GetField(language.ToString())!
          .GetCustomAttribute<DescriptionAttribute>()!.Description).NativeName;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}