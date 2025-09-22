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
        // Initialize the list of supported cultures with English as the default culture
        List<CultureInfo> supportedCultures =
        [
            new("en")
        ];

        // Scan directories in the application's location to find additional supported cultures
        // Each directory name is treated as a culture name (e.g., "zh-CN", "ru-RU", etc.)
        foreach (var directory in Directory.GetDirectories(
                     Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))
            try
            {
                // Create a DirectoryInfo object to get the directory name
                var directoryInfo = new DirectoryInfo(directory);

                // Try to create a CultureInfo object from the directory name and add it to the list
                // If the directory name is not a valid culture name, an exception will be thrown
                supportedCultures.Add(new CultureInfo(directoryInfo.Name));
            }
            catch (Exception)
            {
                // Ignore directories that don't correspond to valid culture names
                // This prevents crashes when encountering non-culture directories
            }

        // Assign the final list of supported cultures to the property
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
        // Get the installed UI culture of the system as the starting point
        var cultureInfo = CultureInfo.InstalledUICulture;

        // Check if the exact culture is supported, if so return it immediately
        if (SupportedCultures.Contains(cultureInfo)) return cultureInfo;

        // Traverse up the culture hierarchy to find a supported parent culture
        // For example, if zh-CN is not supported but zh is supported, we'll use zh
        while (!Equals(cultureInfo.Parent, CultureInfo.InvariantCulture))
        {
            // Check if the current parent culture is supported
            if (SupportedCultures.Contains(cultureInfo)) return cultureInfo;

            // Move to the parent culture (e.g., zh-CN -> zh -> invariant)
            cultureInfo = cultureInfo.Parent;
        }

        // If no supported culture is found in the hierarchy, fall back to English
        return new CultureInfo("en");
    }
}