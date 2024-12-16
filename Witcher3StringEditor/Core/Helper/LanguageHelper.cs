using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Core.Helper;

public static class LanguageHelper
{
    private static readonly Dictionary<string, string> Languages = new()
    {
        { "ar", "cleartext" },
        { "br", "cleartext" },
        { "cn", "cleartext" },
        { "cz", "cz" },
        { "de", "de" },
        { "en", "en" },
        { "es", "es" },
        { "esmx", "cleartext" },
        { "fr", "fr" },
        { "hu", "hu" },
        { "it", "it" },
        { "jp", "jp" },
        { "kr", "cleartext" },
        { "pl", "pl" },
        { "ru", "ru" },
        { "tr", "cleartext" },
        { "zh", "zh" }
    };

    public static string Get(W3Language language)
    {
        return Get(Enum.GetName(language) ?? "en");
    }

    private static string Get(string key)
    {
        return Languages[key];
    }
}