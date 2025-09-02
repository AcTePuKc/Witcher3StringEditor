using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using GTranslate.Translators;
using Microsoft.Extensions.DependencyInjection;

namespace Witcher3StringEditor.Dialogs.Converters;

internal class TranslatorToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string name
            ? DependencyProperty.UnsetValue
            : Ioc.Default.GetServices<ITranslator>().First(x => x.Name == name);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ITranslator || targetType != typeof(string)) return DependencyProperty.UnsetValue;

        return value is ITranslator translator ? translator.Name : DependencyProperty.UnsetValue;
    }
}