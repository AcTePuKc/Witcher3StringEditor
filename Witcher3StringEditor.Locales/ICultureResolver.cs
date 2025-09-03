using System.Globalization;

namespace Witcher3StringEditor.Locales;

public interface ICultureResolver
{
    public IEnumerable<CultureInfo> SupportedCultures { get; }

    CultureInfo ResolveSupportedCulture();
}