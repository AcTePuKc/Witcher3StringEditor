using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Terminology;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class LegacyTranslationRouter : ITranslationRouter
{
    private readonly IAppSettings appSettings;
    private readonly ITerminologyLoader terminologyLoader;
    private readonly ITerminologyPromptBuilder terminologyPromptBuilder;
    private readonly ITranslationProviderRegistry providerRegistry;
    private readonly IEnumerable<ITranslator> legacyTranslators;

    public LegacyTranslationRouter(IAppSettings appSettings,
        ITerminologyLoader terminologyLoader,
        ITerminologyPromptBuilder terminologyPromptBuilder,
        ITranslationProviderRegistry providerRegistry,
        IEnumerable<ITranslator> legacyTranslators)
    {
        this.appSettings = appSettings;
        this.terminologyLoader = terminologyLoader;
        this.terminologyPromptBuilder = terminologyPromptBuilder;
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
                var fallbackReason = providerResult.GetProviderError()
                    ?? providerResult.Errors.FirstOrDefault()?.Message
                    ?? "Provider translation failed.";
                var legacyTranslator = ResolveLegacyTranslator();
                if (legacyTranslator is null)
                {
                    return providerResult;
                }

                Log.Warning(
                    "Provider {ProviderName} failed; falling back to legacy translator {TranslatorName}. Reason: {Reason}",
                    provider.Name,
                    legacyTranslator.Name,
                    fallbackReason);

                var legacyResult = await TranslateWithLegacyTranslator(request, legacyTranslator, cancellationToken)
                    .ConfigureAwait(false);
                return AttachFallbackStatus(legacyResult, fallbackReason);
            }

            return providerResult;
        }

        return await TranslateWithLegacyTranslator(request, cancellationToken).ConfigureAwait(false);
    }

    internal Task<Result<string>> TranslateWithLegacyTranslatorAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        return TranslateWithLegacyTranslator(request, cancellationToken);
    }

    internal Task<Result<string>> TranslateWithProviderAsync(
        TranslationRouterRequest request,
        ITranslationProvider provider,
        CancellationToken cancellationToken = default)
    {
        return TranslateWithProviderAndFallbackAsync(request, provider, cancellationToken);
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
            var metadata = await BuildTerminologyMetadataAsync(request, cancellationToken).ConfigureAwait(false);
            var providerRequest = new TranslationRequest
            {
                Text = request.Text,
                SourceLanguage = request.FromLanguage?.ToString(),
                TargetLanguage = request.ToLanguage?.ToString(),
                ModelId = ResolveModelName(request),
                Metadata = metadata
            };

            var response = await provider.TranslateAsync(providerRequest, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(response.TranslatedText))
            {
                return Result.Fail(BuildProviderFailure(provider.Name,
                    $"Translation provider '{provider.Name}' returned an empty result.",
                    TranslationProviderFailureKind.InvalidResponse));
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
            return Result.Fail(BuildProviderFailure(
                provider.Name,
                $"Translation provider '{provider.Name}' failed with an exception: {ex.Message}.",
                ResolveFailureKind(ex),
                ex));
        }
    }

    private async Task<Result<string>> TranslateWithLegacyTranslator(
        TranslationRouterRequest request,
        CancellationToken cancellationToken)
    {
        var translator = ResolveLegacyTranslator();
        if (translator is null)
            return Result.Fail("No legacy translators are registered.");

        return await TranslateWithLegacyTranslator(request, translator, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Result<string>> TranslateWithLegacyTranslator(
        TranslationRouterRequest request,
        ITranslator translator,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result.Fail("Translation was cancelled.");

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

    private static Result<string> AttachFallbackStatus(Result<string> result, string fallbackReason)
    {
        var statusMessage = $"Provider failed; using legacy translator. Reason: {fallbackReason}";
        var fallbackStatus = new Success("Translation provider fallback was used.")
            .WithMetadata(TranslationStatusMetadata.StatusMessageKey, statusMessage);
        return result.WithSuccess(fallbackStatus);
    }

    private async Task<IReadOnlyDictionary<string, string>?> BuildTerminologyMetadataAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.UseProviderForTranslation)
        {
            return null;
        }

        var context = request.PipelineContext;
        if (context is null)
        {
            return null;
        }

        if (context.TerminologyPaths.Count == 0 && string.IsNullOrWhiteSpace(context.StyleGuidePath))
        {
            return null;
        }

        TerminologyPack? terminologyPack = null;
        TerminologyPack? styleGuidePack = null;

        try
        {
            if (context.TerminologyPaths.Count > 0)
            {
                var loadedPacks = new List<TerminologyPack>();
                foreach (var path in context.TerminologyPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    loadedPacks.Add(await terminologyLoader.LoadAsync(path, cancellationToken).ConfigureAwait(false));
                }

                terminologyPack = MergeTerminologyPacks(loadedPacks);
            }

            if (!string.IsNullOrWhiteSpace(context.StyleGuidePath))
            {
                styleGuidePack = await terminologyLoader.LoadAsync(context.StyleGuidePath, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warning(ex, "Failed to build terminology prompt metadata for provider routing.");
            return null;
        }

        var prompt = await terminologyPromptBuilder.BuildAsync(
            terminologyPack,
            styleGuidePack,
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(prompt.SystemPrompt) && string.IsNullOrWhiteSpace(prompt.UserPrompt))
        {
            return null;
        }

        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(prompt.SystemPrompt))
        {
            metadata[TerminologyPromptMetadata.SystemPromptKey] = prompt.SystemPrompt;
        }

        if (!string.IsNullOrWhiteSpace(prompt.UserPrompt))
        {
            metadata[TerminologyPromptMetadata.UserPromptKey] = prompt.UserPrompt;
        }

        return metadata;
    }

    private static TerminologyPack? MergeTerminologyPacks(IReadOnlyList<TerminologyPack> packs)
    {
        if (packs.Count == 0)
        {
            return null;
        }

        if (packs.Count == 1)
        {
            return packs[0];
        }

        var entries = packs
            .SelectMany(pack => pack.Entries ?? Array.Empty<TerminologyEntry>())
            .ToList();
        var sourcePath = string.Join(";", packs.Select(pack => pack.SourcePath).Where(path => !string.IsNullOrWhiteSpace(path)));

        return new TerminologyPack
        {
            Name = "Combined terminology",
            SourcePath = sourcePath,
            Entries = entries
        };
    }

    private async Task<Result<string>> TranslateWithProviderAndFallbackAsync(
        TranslationRouterRequest request,
        ITranslationProvider provider,
        CancellationToken cancellationToken)
    {
        var providerResult = await TranslateWithProvider(request, provider, cancellationToken).ConfigureAwait(false);
        if (providerResult.IsSuccess)
        {
            return providerResult;
        }

        if (!ShouldFallbackToLegacyTranslator())
        {
            return providerResult;
        }

        var fallbackReason = providerResult.GetProviderError()
            ?? providerResult.Errors.FirstOrDefault()?.Message
            ?? "Provider translation failed.";
        var legacyTranslator = ResolveLegacyTranslator();
        if (legacyTranslator is null)
        {
            return providerResult;
        }

        Log.Warning(
            "Provider {ProviderName} failed; falling back to legacy translator {TranslatorName}. Reason: {Reason}",
            provider.Name,
            legacyTranslator.Name,
            fallbackReason);

        var legacyResult = await TranslateWithLegacyTranslator(request, legacyTranslator, cancellationToken)
            .ConfigureAwait(false);
        return AttachFallbackStatus(legacyResult, fallbackReason);
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
        return legacyTranslators.Any();
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

    private static Error BuildProviderFailure(
        string providerName,
        string message,
        TranslationProviderFailureKind failureKind,
        Exception? exception = null)
    {
        var statusMessage = $"{message} (Failure kind: {failureKind})";
        var error = new Error(message)
            .WithMetadata(TranslationFailureMetadata.FailureKindKey, TranslationFailureMetadata.ProviderFailureKind)
            .WithMetadata(TranslationFailureMetadata.ProviderFailureReasonKey, failureKind)
            .WithMetadata(TranslationStatusMetadata.StatusMessageKey, statusMessage)
            .WithMetadata(TranslationFailureMetadata.ProviderNameKey, providerName);

        if (exception is not null)
        {
            error = error.WithMetadata(TranslationFailureMetadata.ExceptionTypeKey, exception.GetType().Name);
        }

        return error;
    }

    private static TranslationProviderFailureKind ResolveFailureKind(Exception exception)
    {
        return exception switch
        {
            TimeoutException => TranslationProviderFailureKind.Timeout,
            HttpRequestException => TranslationProviderFailureKind.Network,
            _ => TranslationProviderFailureKind.Unknown
        };
    }
}
