using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationRouter : ITranslationRouter
{
    private readonly IAppSettings appSettings;
    private readonly ITranslationProviderRegistry providerRegistry;
    private readonly IEnumerable<ITranslator> legacyTranslators;

    public TranslationRouter(IAppSettings appSettings,
        ITranslationProviderRegistry providerRegistry,
        IEnumerable<ITranslator> legacyTranslators)
    {
        this.appSettings = appSettings;
        this.providerRegistry = providerRegistry;
        this.legacyTranslators = legacyTranslators;
    }

    public async Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (TryResolveProvider(request, out var provider))
        {
            var providerResult = await TranslateWithProvider(request, provider, cancellationToken)
                .ConfigureAwait(false);
            if (providerResult.IsSuccess)
            {
                return providerResult;
            }

            if (ShouldFallbackToLegacyTranslator())
            {
                Log.Warning("Provider {ProviderName} failed; falling back to legacy translator.", provider.Name);
                return await TranslateWithLegacyTranslator(request, cancellationToken).ConfigureAwait(false);
            }

            return providerResult;
        }

        return await TranslateWithLegacyTranslator(request, cancellationToken).ConfigureAwait(false);
    }

    private bool TryResolveProvider(TranslationRouterRequest request, out ITranslationProvider? provider)
    {
        provider = null;
        var providerName = ResolveProviderName(request);
        if (string.IsNullOrWhiteSpace(providerName))
            return false;

        provider = providerRegistry.Resolve(providerName);
        return provider is not null;
    }

    private async Task<Result<string>> TranslateWithProvider(
        TranslationRouterRequest request,
        ITranslationProvider provider,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result.Fail("Translation was cancelled.");

        try
        {
            var providerRequest = new TranslationRequest
            {
                Text = request.Text,
                SourceLanguage = request.FromLanguage?.ToString(),
                TargetLanguage = request.ToLanguage?.ToString(),
                ModelId = ResolveModelName(request)
            };

            var response = await provider.TranslateAsync(providerRequest, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(response.TranslatedText))
            {
                return Result.Fail(BuildProviderFailure(provider.Name,
                    $"Translation provider '{provider.Name}' returned an empty result."));
            }

            return Result.Ok(response.TranslatedText);
        }
        catch (OperationCanceledException)
        {
            return Result.Fail("Translation was cancelled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Translation provider {ProviderName} failed with an exception.", provider.Name);
            return Result.Fail(BuildProviderFailure(provider.Name,
                $"Translation provider '{provider.Name}' failed with an exception: {ex.Message}.", ex));
        }
    }

    private async Task<Result<string>> TranslateWithLegacyTranslator(
        TranslationRouterRequest request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result.Fail("Translation was cancelled.");

        var translator = ResolveLegacyTranslator();
        if (translator is null)
            return Result.Fail("No legacy translators are registered.");

        try
        {
            var translation = await translator
                .TranslateAsync(request.Text, request.ToLanguage, request.FromLanguage)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(translation.Translation))
                return Result.Fail($"Translator '{translator.Name}' returned an empty result.");

            return Result.Ok(translation.Translation);
        }
        finally
        {
            if (translator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private ITranslator? ResolveLegacyTranslator()
    {
        var translatorName = appSettings.Translator;
        var translator = string.IsNullOrWhiteSpace(translatorName)
            ? legacyTranslators.FirstOrDefault()
            : legacyTranslators.FirstOrDefault(candidate => candidate.Name == translatorName);

        return translator ?? legacyTranslators.FirstOrDefault();
    }

    private bool ShouldFallbackToLegacyTranslator()
    {
        return !string.IsNullOrWhiteSpace(appSettings.Translator) && legacyTranslators.Any();
    }

    private string? ResolveProviderName(TranslationRouterRequest request)
    {
        return ResolveOverride(request.ProviderName, appSettings.TranslationProviderName);
    }

    private string? ResolveModelName(TranslationRouterRequest request)
    {
        return ResolveOverride(request.ModelName, appSettings.TranslationModelName);
    }

    private static string? ResolveOverride(string? overrideValue, string? fallbackValue)
    {
        return string.IsNullOrWhiteSpace(overrideValue) ? fallbackValue : overrideValue;
    }

    private static Error BuildProviderFailure(string providerName, string message, Exception? exception = null)
    {
        var error = new Error(message)
            .WithMetadata(TranslationFailureMetadata.FailureKindKey, TranslationFailureMetadata.ProviderFailureKind)
            .WithMetadata(TranslationFailureMetadata.ProviderNameKey, providerName);

        if (exception is not null)
        {
            error = error.WithMetadata(TranslationFailureMetadata.ExceptionTypeKey, exception.GetType().Name);
        }

        return error;
    }
}
