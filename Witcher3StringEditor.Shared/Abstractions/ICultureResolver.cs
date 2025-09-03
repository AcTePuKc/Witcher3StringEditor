using System.Globalization;

namespace Witcher3StringEditor.Shared.Abstractions;

public interface ICultureResolver
{
    public IEnumerable<CultureInfo> SupportedCultures { get; }

    CultureInfo ResolveSupportedCulture();
}