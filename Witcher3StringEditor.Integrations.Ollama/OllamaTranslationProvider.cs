using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed class OllamaTranslationProvider : ITranslationProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly OllamaSettings settings;

    public OllamaTranslationProvider(HttpClient httpClient, OllamaSettings settings)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string Name => "Ollama";

    public async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = string.IsNullOrWhiteSpace(settings.BaseUrl)
            ? "http://localhost:11434"
            : settings.BaseUrl;
        var baseUri = new Uri(baseUrl, UriKind.Absolute);
        var requestUri = new Uri(baseUri, "api/tags");

        using var response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var responseStream =
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responsePayload = await JsonSerializer.DeserializeAsync<OllamaModelListResponse>(
            responseStream,
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        if (responsePayload?.Models is null || responsePayload.Models.Count == 0)
        {
            return Array.Empty<ModelInfo>();
        }

        var results = new List<ModelInfo>(responsePayload.Models.Count);
        foreach (var model in responsePayload.Models)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                continue;
            }

            var metadata = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(model.Digest))
            {
                metadata["digest"] = model.Digest;
            }

            if (model.Size.HasValue)
            {
                metadata["size"] = model.Size.Value.ToString();
            }

            results.Add(new ModelInfo
            {
                Id = model.Name,
                DisplayName = model.Name,
                Metadata = metadata.Count > 0 ? metadata : null
            });
        }

        return results;
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
