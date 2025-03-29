using CommunityToolkit.Diagnostics;
using Serilog;
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
            var encryptedPassword = value as string;
            if(string.IsNullOrWhiteSpace(encryptedPassword)) return DependencyProperty.UnsetValue;
            var encryptedData = System.Convert.FromBase64String(encryptedPassword);
            var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to decrypt password");
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var password = value as string;
            if (string.IsNullOrWhiteSpace(password)) return DependencyProperty.UnsetValue;
            var data = Encoding.UTF8.GetBytes(password);
            var encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return System.Convert.ToBase64String(encryptedData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to encrypt password");
            return DependencyProperty.UnsetValue;
        }
    }
}