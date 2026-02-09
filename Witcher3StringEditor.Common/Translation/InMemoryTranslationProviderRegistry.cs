using System;
using System.Collections.Generic;
using System.Linq;

namespace Witcher3StringEditor.Common.Translation;

/// <summary>
///     In-memory provider registry stub for future wiring. Not used at runtime by default.
/// </summary>
public sealed class InMemoryTranslationProviderRegistry : ITranslationProviderRegistry
{
    private readonly Dictionary<string, ITranslationProvider> providers =
        new(StringComparer.OrdinalIgnoreCase);

    public InMemoryTranslationProviderRegistry()
    {
    }

    public InMemoryTranslationProviderRegistry(IEnumerable<ITranslationProvider> providers)
    {
        if (providers is null)
            throw new ArgumentNullException(nameof(providers));

        foreach (var provider in providers)
            Register(provider);
    }

    public void Register(ITranslationProvider provider)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        providers[provider.Name] = provider;
    }

    public IReadOnlyList<TranslationProviderDescriptor> GetProviders()
    {
        return providers.Values
            .Select(provider => new TranslationProviderDescriptor
            {
                Name = provider.Name,
                DisplayName = provider.Name,
                SupportsModelListing = false,
                Notes = "Stub registry; model listing support not inferred yet."
            })
            .ToList();
    }

    public ITranslationProvider? Resolve(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return null;

        return providers.TryGetValue(providerName, out var provider)
            ? provider
            : null;
    }

    public bool TryGet(string providerName, out ITranslationProvider provider)
    {
        if (providerName is null)
            throw new ArgumentNullException(nameof(providerName));

        return providers.TryGetValue(providerName, out provider!);
    }
}
