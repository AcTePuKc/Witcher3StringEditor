using System;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationProviderHealthCheck : ITranslationProviderHealthCheck
{
    private readonly ITranslationProviderRegistry providerRegistry;

    public TranslationProviderHealthCheck(ITranslationProviderRegistry providerRegistry)
    {
        this.providerRegistry = providerRegistry ?? throw new ArgumentNullException(nameof(providerRegistry));
    }

    public async Task<TranslationProviderHealthCheckResult> CheckAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return new TranslationProviderHealthCheckResult(false, "Select a provider before testing the connection.");
        }

        if (!providerRegistry.TryGet(providerName, out var provider))
        {
            return new TranslationProviderHealthCheckResult(
                false,
                $"Provider '{providerName}' is not registered yet.");
        }

        try
        {
            var models = await provider.ListModelsAsync(cancellationToken).ConfigureAwait(false);
            var modelCount = models?.Count ?? 0;
            var message = modelCount > 0
                ? $"Connected. {modelCount} models available."
                : "Connected, but no models were returned.";

            return new TranslationProviderHealthCheckResult(true, message, modelCount);
        }
        catch (Exception ex)
        {
            return new TranslationProviderHealthCheckResult(
                false,
                $"Connection test failed: {ex.Message}");
        }
    }
}
