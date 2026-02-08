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
}
