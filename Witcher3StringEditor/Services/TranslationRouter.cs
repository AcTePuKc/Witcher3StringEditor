using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationRouter : ITranslationRouter
{
    private readonly ITranslationProviderRegistry providerRegistry;
    private readonly LegacyTranslationRouter legacyRouter;
    private readonly IAppSettings appSettings;

    public TranslationRouter(
        ITranslationProviderRegistry providerRegistry,
        LegacyTranslationRouter legacyRouter,
        IAppSettings appSettings)
    {
        this.providerRegistry = providerRegistry ?? throw new ArgumentNullException(nameof(providerRegistry));
        this.legacyRouter = legacyRouter ?? throw new ArgumentNullException(nameof(legacyRouter));
        this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public async Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (!request.UseProviderForTranslation)
        {
            return await legacyRouter.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var validationResult = ValidateProviderRequest(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        if (string.IsNullOrWhiteSpace(request.ProviderName))
        {
            return await legacyRouter.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var provider = providerRegistry.Resolve(request.ProviderName);
        if (provider is null)
        {
            var fallbackReason = $"Provider '{request.ProviderName}' was not found.";
            if (!appSettings.UseLegacyTranslationFallback)
            {
                return Result.Fail(BuildProviderRequestFailure(
                    $"Translation provider '{request.ProviderName}' is not available and legacy fallback is disabled.",
                    TranslationProviderFailureKind.Unknown,
                    request.ProviderName));
            }

            Log.Warning(
                "Translation provider {ProviderName} was not found; falling back to legacy translator.",
                request.ProviderName);
            var legacyResult = await legacyRouter.TranslateWithLegacyTranslatorAsync(request, cancellationToken)
                .ConfigureAwait(false);
            return AttachFallbackStatus(legacyResult, fallbackReason);
        }

        return await legacyRouter.TranslateWithProviderAsync(request, provider, cancellationToken)
            .ConfigureAwait(false);
    }

    private static Result<string>? ValidateProviderRequest(TranslationRouterRequest request)
    {
        if (!request.UseProviderForTranslation)
        {
            return null;
        }

        var missingProvider = string.IsNullOrWhiteSpace(request.ProviderName);
        var missingModel = string.IsNullOrWhiteSpace(request.ModelName);

        if (!missingProvider && !missingModel)
        {
            return null;
        }

        var message = missingProvider && missingModel
            ? "Select a translation provider and model before using provider routing."
            : missingProvider
                ? "Select a translation provider before using provider routing."
                : "Select a translation model before using provider routing.";

        var failureKind = missingModel
            ? TranslationProviderFailureKind.MissingModel
            : TranslationProviderFailureKind.Unknown;

        return Result.Fail(BuildProviderRequestFailure(message, failureKind));
    }

    private static Error BuildProviderRequestFailure(
        string message,
        TranslationProviderFailureKind failureKind,
        string? providerName = null)
    {
        var statusMessage = $"{message} (Failure kind: {failureKind})";
        var error = new Error(message)
            .WithMetadata(TranslationFailureMetadata.FailureKindKey,
                TranslationFailureMetadata.RequestValidationFailureKind)
            .WithMetadata(TranslationFailureMetadata.ProviderFailureReasonKey, failureKind)
            .WithMetadata(TranslationStatusMetadata.StatusMessageKey, statusMessage);

        if (!string.IsNullOrWhiteSpace(providerName))
        {
            error = error.WithMetadata(TranslationFailureMetadata.ProviderNameKey, providerName);
        }

        return error;
    }

    private static Result<string> AttachFallbackStatus(Result<string> result, string fallbackReason)
    {
        if (result is null)
        {
            return result;
        }

        var statusMessage = $"Provider fallback used; using legacy translator. Reason: {fallbackReason}";
        var fallbackStatus = new Success("Translation provider fallback was used.")
            .WithMetadata(TranslationStatusMetadata.StatusMessageKey, statusMessage);
        return result.WithSuccess(fallbackStatus);
    }
}
