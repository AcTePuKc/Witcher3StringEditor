using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationProviderRegistry
{
    IReadOnlyList<TranslationProviderDescriptor> GetProviders();

    ITranslationProvider? Resolve(string providerName);

    bool TryGet(string providerName, out ITranslationProvider provider);
}

public sealed class TranslationProviderDescriptor
{
    public string Name { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    public bool SupportsModelListing { get; init; }

    public string? Notes { get; init; }
}
