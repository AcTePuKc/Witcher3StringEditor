using Serilog.Events;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters
{
    internal class LogEventToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogEvent logEvent)
            {
                // 调用 A 类中的方法
                return logEvent.RenderMessage();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}