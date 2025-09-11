using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.Converters;

internal class W3FileTypeToLocalizedStringConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not W3FileType fileType) return DependencyProperty.UnsetValue;
        return fileType switch
        {
            W3FileType.Csv => Strings.FileFormatTextFile,
            W3FileType.W3Strings => Strings.FileFormatWitcher3StringsFile,
            W3FileType.Excel => Strings.FileFormatExcelWorkbook,
            _ => DependencyProperty.UnsetValue
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}