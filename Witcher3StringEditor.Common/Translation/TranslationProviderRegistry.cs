using System;
using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Translation;

// TODO: Consolidate with Witcher3StringEditor.Services.TranslationProviderRegistry once legacy registry usage is retired.
public sealed class TranslationProviderRegistry
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, ITranslationProvider> providers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(ITranslationProvider provider)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        providers[provider.Name] = provider;
    }

    public ITranslationProvider Get(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        if (providers.TryGetValue(name, out var provider))
            return provider;

        throw new KeyNotFoundException($"Translation provider '{name}' was not found.");
    }

    public bool TryGet(string name, out ITranslationProvider? provider)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        return providers.TryGetValue(name, out provider);
    }
}
