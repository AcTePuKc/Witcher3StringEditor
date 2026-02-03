using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed class OllamaTranslationProvider : ITranslationProvider
{
    private readonly HttpClient httpClient;
    private readonly OllamaSettings settings;

    public OllamaTranslationProvider(HttpClient httpClient, OllamaSettings settings)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (this.httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(this.settings.BaseUrl))
        {
            this.httpClient.BaseAddress = new Uri(this.settings.BaseUrl, UriKind.Absolute);
        }

        if (this.settings.Timeout is { } timeout)
        {
            this.httpClient.Timeout = timeout;
        }
    }

    public string Name => "Ollama";

    public async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/tags", cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: Surface errors to the caller once a provider diagnostics pipeline exists.
            return Array.Empty<ModelInfo>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!document.RootElement.TryGetProperty("models", out var modelsElement) ||
            modelsElement.ValueKind != JsonValueKind.Array)
        {
            // TODO: Confirm the response shape for /api/tags in the target Ollama version.
            return Array.Empty<ModelInfo>();
        }

        return modelsElement.EnumerateArray()
            .Select(modelElement =>
                modelElement.TryGetProperty("name", out var nameElement) &&
                nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()
                    : null)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => new ModelInfo
            {
                Id = id!,
                DisplayName = id,
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "ollama:/api/tags"
                }
            })
            .ToList();
    }

    public Task<TranslationResult> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // TODO: Implement /api/generate or /api/chat call once prompt format and response handling are defined.
        var modelId = string.IsNullOrWhiteSpace(request.ModelId) ? settings.ModelName : request.ModelId;

        return Task.FromResult(new TranslationResult
        {
            TranslatedText = request.Text,
            ModelId = modelId,
            Metadata = new Dictionary<string, string>
            {
                ["todo"] = "Ollama translation call not implemented yet."
            }
        });
    }
}
