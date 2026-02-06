using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Translation;
using Witcher3StringEditor.Dialogs.Services;

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
}
