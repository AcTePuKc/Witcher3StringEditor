using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed class OllamaModelCatalog : ITranslationModelCatalog
{
    private readonly ITranslationProvider provider;

    public OllamaModelCatalog(ITranslationProvider provider)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public Task<IReadOnlyList<ModelInfo>> GetAsync(string providerName, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(providerName, provider.Name, StringComparison.OrdinalIgnoreCase))
        {
            IReadOnlyList<ModelInfo> empty = Array.Empty<ModelInfo>();
            return Task.FromResult(empty);
        }

        // TODO: Replace the provider stub with real Ollama model discovery once API wiring is approved.
        return provider.ListModelsAsync(cancellationToken);
    }
}
