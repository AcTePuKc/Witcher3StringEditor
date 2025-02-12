using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Witcher3StringEditor.Dialogs.Converters;

public class SecurePasswordConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string encryptedPassword || string.IsNullOrWhiteSpace(encryptedPassword))
                return DependencyProperty.UnsetValue;
            var encryptedData = System.Convert.FromBase64String(encryptedPassword);
            var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch (CryptographicException ex)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string password || string.IsNullOrWhiteSpace(password))
                return DependencyProperty.UnsetValue;
            var data = Encoding.UTF8.GetBytes(password);
            var encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return System.Convert.ToBase64String(encryptedData);
        }
        catch (CryptographicException ex)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}