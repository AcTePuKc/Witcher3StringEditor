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
        // TODO: Replace with a real registry that maps provider names to implementations.
        _ = providerName;
        return null;
    }

    public bool TryGet(string providerName, out ITranslationProvider provider)
    {
        // TODO: Replace with a real registry that maps provider names to implementations.
        _ = providerName;
        provider = null!;
        return false;
    }
}
