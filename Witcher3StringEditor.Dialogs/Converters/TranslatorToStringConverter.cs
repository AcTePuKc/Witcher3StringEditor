using CommunityToolkit.Mvvm.DependencyInjection;
using GTranslate.Translators;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

internal class TranslatorToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string name)
        {
            return DependencyProperty.UnsetValue;
        }
        else
        {
            var translator = Ioc.Default.GetServices<ITranslator>().First(x => x.Name == name);
            return translator ?? DependencyProperty.UnsetValue;
        }

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ITranslator || targetType != typeof(string))
        {
            return DependencyProperty.UnsetValue;
        }
        else
        {
            return value is ITranslator translator ? translator.Name : DependencyProperty.UnsetValue;
        }
    }
}
