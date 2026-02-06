using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Translation;
using Witcher3StringEditor.Dialogs.Services;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationRouter : ITranslationRouter
{
    private readonly IAppSettings appSettings;
    private readonly ITranslationProviderRegistry providerRegistry;

    public TranslationRouter(IAppSettings appSettings, ITranslationProviderRegistry providerRegistry)
    {
        this.appSettings = appSettings;
        this.providerRegistry = providerRegistry;
    }

    public async Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (TryResolveProvider(out var provider))
        {
            return await TranslateWithProvider(request, provider, cancellationToken).ConfigureAwait(false);
        }

        return await TranslateWithLegacyTranslator(request, cancellationToken).ConfigureAwait(false);
    }

    private bool TryResolveProvider(out ITranslationProvider? provider)
    {
        provider = null;
        var providerName = appSettings.TranslationProviderName;
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
        _ = provider;
        _ = cancellationToken;

        // TODO: Wire provider requests once translation providers are fully implemented.
        Log.Warning("Translation provider path is not implemented yet.");
        return Result.Fail("Translation provider path is not implemented yet.");
    }

    private static async Task<Result<string>> TranslateWithLegacyTranslator(
        TranslationRouterRequest request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result.Fail("Translation was cancelled.");

        var translation = await request.LegacyTranslator
            .TranslateAsync(request.Text, request.ToLanguage, request.FromLanguage)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(translation.Translation))
            return Result.Fail("Translator returned an empty result.");

        return Result.Ok(translation.Translation);
    }
}
