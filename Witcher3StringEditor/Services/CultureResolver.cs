using System.Globalization;
using System.IO;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Services;

public class CultureResolver: ICultureResolver
{
    public IEnumerable<CultureInfo> SupportedCultures { get; }

    public CultureResolver()
    {
        List<CultureInfo> supportedCultures = [];
        foreach (var directory in Directory.GetDirectories(Directory.GetCurrentDirectory()))
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directory);
                supportedCultures.Add(new CultureInfo(directoryInfo.Name));
            }
            catch (Exception)
            {
                //ignored
            }
        }
        SupportedCultures = supportedCultures;
    }
    
    public CultureInfo ResolveSupportedCulture()
    {
        var cultureInfo = CultureInfo.InstalledUICulture;
        if (SupportedCultures.Contains(cultureInfo)) return cultureInfo;
        while (!Equals(cultureInfo.Parent, CultureInfo.InvariantCulture))
        {
            if (SupportedCultures.Contains(cultureInfo)) return cultureInfo;
            cultureInfo = cultureInfo.Parent;
        }
        return new CultureInfo("en");
    }
}