using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Serilog;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationRouter : ITranslationRouter
{
    private readonly ITranslationProviderRegistry providerRegistry;
    private readonly LegacyTranslationRouter legacyRouter;

    public TranslationRouter(ITranslationProviderRegistry providerRegistry, LegacyTranslationRouter legacyRouter)
    {
        this.providerRegistry = providerRegistry ?? throw new ArgumentNullException(nameof(providerRegistry));
        this.legacyRouter = legacyRouter ?? throw new ArgumentNullException(nameof(legacyRouter));
    }

    public Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var validationResult = ValidateProviderRequest(request);
        if (validationResult is not null)
        {
            return Task.FromResult(validationResult);
        }

        if (string.IsNullOrWhiteSpace(request.ProviderName))
        {
            return legacyRouter.TranslateAsync(request, cancellationToken);
        }

        var provider = providerRegistry.Resolve(request.ProviderName);
        if (provider is null)
        {
            Log.Warning(
                "Translation provider {ProviderName} was not found; falling back to legacy translator.",
                request.ProviderName);
            return legacyRouter.TranslateWithLegacyTranslatorAsync(request, cancellationToken);
        }

        return legacyRouter.TranslateWithProviderAsync(request, provider, cancellationToken);
    }

    private static Result<string>? ValidateProviderRequest(TranslationRouterRequest request)
    {
        var shouldValidate = request.UseProviderForTranslation || !string.IsNullOrWhiteSpace(request.ProviderName);
        if (!shouldValidate)
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

        return Result.Fail(BuildProviderRequestFailure(message));
    }

    private static Error BuildProviderRequestFailure(string message)
    {
        return new Error(message)
            .WithMetadata(TranslationFailureMetadata.FailureKindKey,
                TranslationFailureMetadata.RequestValidationFailureKind)
            .WithMetadata(TranslationStatusMetadata.StatusMessageKey, message);
    }
}
