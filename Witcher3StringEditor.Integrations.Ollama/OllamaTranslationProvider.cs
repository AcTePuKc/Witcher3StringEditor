using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed class OllamaTranslationProvider : ITranslationProvider
{
    private readonly OllamaSettings settings;

    public OllamaTranslationProvider(HttpClient httpClient, OllamaSettings settings)
    {
        _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string Name => "Ollama";

    public async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(settings.ModelName))
        {
            return Array.Empty<ModelInfo>();
        }

        return new[]
        {
            new ModelInfo
            {
                Id = settings.ModelName,
                DisplayName = settings.ModelName,
                Metadata = new Dictionary<string, string>
                {
                    ["stub"] = "true",
                    ["note"] = "Ollama model listing is not implemented yet."
                }
            }
        };
    }

    public Task<TranslationResult> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var modelId = string.IsNullOrWhiteSpace(request.ModelId) ? settings.ModelName : request.ModelId;

        return Task.FromResult(new TranslationResult
        {
            TranslatedText = request.Text,
            ModelId = modelId,
            Metadata = new Dictionary<string, string>
            {
                ["stub"] = "true",
                ["note"] = "Ollama translation call not implemented yet."
            }
        });
    }
}
