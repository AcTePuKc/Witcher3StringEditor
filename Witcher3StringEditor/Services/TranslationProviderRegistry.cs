using System;
using System.Collections.Generic;
using System.Linq;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationProviderRegistry : ITranslationProviderRegistry
{
    private readonly IReadOnlyList<TranslationProviderDescriptor> descriptors;
    private readonly Dictionary<string, ITranslationProvider> providerMap;

    public TranslationProviderRegistry(IEnumerable<ITranslationProvider> providers)
    {
        var providerList = (providers ?? Array.Empty<ITranslationProvider>())
            .Where(provider => !string.IsNullOrWhiteSpace(provider.Name))
            .ToList();

        providerMap = providerList
            .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(grouping => grouping.Key, grouping =>
            {
                if (grouping.Skip(1).Any())
                {
                    throw new ArgumentException(
                        $"Duplicate translation provider name '{grouping.Key}' found. Provider names must be unique (case-insensitive).");
                }

                return grouping.First();
            }, StringComparer.OrdinalIgnoreCase);

        descriptors = providerMap.Values
            .Select(provider => new TranslationProviderDescriptor
            {
                Name = provider.Name,
                DisplayName = provider.Name,
                SupportsModelListing = false
            })
            .OrderBy(descriptor => descriptor.DisplayName ?? descriptor.Name)
            .ToList();
    }

    public IReadOnlyList<TranslationProviderDescriptor> GetProviders()
    {
        return descriptors;
    }

    public ITranslationProvider? Resolve(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return providerMap.TryGetValue(providerName, out var provider) ? provider : null;
    }
}
