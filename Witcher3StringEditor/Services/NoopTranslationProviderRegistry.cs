using System;
using System.Collections.Generic;
using System.Linq;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProviderRegistry : ITranslationProviderRegistry
{
    private static readonly IReadOnlyList<TranslationProviderDescriptor> EmptyProviders = [];

    public IReadOnlyList<TranslationProviderDescriptor> GetProviders()
    {
        // TODO: Replace with a real registry that maps provider names to implementations.
        return EmptyProviders;
    }

    public ITranslationProvider? Resolve(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return EmptyProviders
            .FirstOrDefault(provider => string.Equals(provider.Name, providerName, StringComparison.OrdinalIgnoreCase))
            is { } descriptor
            ? ResolveDescriptor(descriptor)
            : null;
    }

    private static ITranslationProvider? ResolveDescriptor(TranslationProviderDescriptor descriptor)
    {
        // TODO: Resolve from DI once providers are wired.
        _ = descriptor;
        return null;
    }
}
