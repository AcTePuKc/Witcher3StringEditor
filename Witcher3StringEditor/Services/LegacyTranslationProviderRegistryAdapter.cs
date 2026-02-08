using System;
using System.Collections.Generic;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class LegacyTranslationProviderRegistryAdapter : ITranslationProviderRegistry
{
    private readonly TranslationProviderRegistry legacyRegistry;

    public LegacyTranslationProviderRegistryAdapter(TranslationProviderRegistry legacyRegistry)
    {
        this.legacyRegistry = legacyRegistry ?? throw new ArgumentNullException(nameof(legacyRegistry));
    }

    public IReadOnlyList<TranslationProviderDescriptor> GetProviders()
    {
        // TODO: Surface legacy providers with descriptors if legacy usage is confirmed.
        return Array.Empty<TranslationProviderDescriptor>();
    }

    public ITranslationProvider? Resolve(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return legacyRegistry.TryGet(providerName, out var provider) ? provider : null;
    }

    public bool TryGet(string providerName, out ITranslationProvider provider)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            provider = null!;
            return false;
        }

        if (legacyRegistry.TryGet(providerName, out var resolved) && resolved is not null)
        {
            provider = resolved;
            return true;
        }

        provider = null!;
        return false;
    }
}
