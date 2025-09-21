using System.Globalization;
using System.IO;
using System.Reflection;

namespace Witcher3StringEditor.Locales;

/// <summary>
///     Provides culture resolution functionality for the application
///     Detects and resolves supported cultures based on available resource directories
/// </summary>
public class CultureResolver : ICultureResolver
{
    /// <summary>
    ///     Initializes a new instance of the CultureResolver class
    ///     Scans the application directory for culture-specific resource folders
    ///     and builds a list of supported cultures
    /// </summary>
    public CultureResolver()
    {
        List<CultureInfo> supportedCultures =
        [
            new("en")
        ];
        foreach (var directory in Directory.GetDirectories(
                     Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))
            try
            {
                var directoryInfo = new DirectoryInfo(directory);
                supportedCultures.Add(new CultureInfo(directoryInfo.Name));
            }
            catch (Exception)
            {
                //ignored
            }

        SupportedCultures = supportedCultures;
    }

    /// <summary>
    ///     Gets the collection of cultures supported by the application
    ///     This is determined by the presence of culture-specific resource directories
    /// </summary>
    public IEnumerable<CultureInfo> SupportedCultures { get; }

    /// <summary>
    ///     Resolves the most appropriate supported culture for the current system
    ///     Tries to match the installed UI culture, and falls back through parent cultures
    ///     If no match is found, defaults to English ("en")
    /// </summary>
    /// <returns>The best matching supported culture, or English as fallback</returns>
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