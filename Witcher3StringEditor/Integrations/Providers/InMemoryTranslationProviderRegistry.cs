using System;
using System.Collections.Generic;

namespace Witcher3StringEditor.Integrations.Providers;

public sealed class InMemoryTranslationProviderRegistry : ITranslationProviderRegistry
{
    private readonly Dictionary<string, ITranslationProvider> _providers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ITranslationProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        _providers[provider.Name] = provider;
    }

    public bool TryGet(string name, out ITranslationProvider provider)
        => _providers.TryGetValue(name, out provider!);

    public IReadOnlyCollection<ITranslationProvider> GetAll()
        => _providers.Values;
}
