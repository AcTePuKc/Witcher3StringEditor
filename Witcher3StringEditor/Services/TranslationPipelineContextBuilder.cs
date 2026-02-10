using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Profiles;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationPipelineContextBuilder : ITranslationPipelineContextBuilder
{
    private readonly IAppSettings appSettings;
    private readonly ITranslationProfileResolver profileResolver;

    public TranslationPipelineContextBuilder(IAppSettings appSettings, ITranslationProfileResolver profileResolver)
    {
        this.appSettings = appSettings;
        this.profileResolver = profileResolver;
    }

    public async Task<TranslationPipelineContext> BuildAsync(CancellationToken cancellationToken = default)
    {
        var profile = await profileResolver.ResolveAsync(appSettings.TranslationProfileId, cancellationToken)
            .ConfigureAwait(false);

        var providerId = !string.IsNullOrWhiteSpace(profile?.ProviderName)
            ? profile.ProviderName
            : appSettings.TranslationProviderName;

        var modelId = !string.IsNullOrWhiteSpace(profile?.ModelName)
            ? profile.ModelName
            : appSettings.TranslationModelName;

        var terminologyPaths = new List<string>();
        if (!string.IsNullOrWhiteSpace(profile?.TerminologyPath))
        {
            terminologyPaths.Add(profile.TerminologyPath);
        }

        if (!string.IsNullOrWhiteSpace(appSettings.TerminologyFilePath))
        {
            terminologyPaths.Add(appSettings.TerminologyFilePath);
        }

        return new TranslationPipelineContext
        {
            ProfileId = profile?.Id,
            ProviderId = providerId,
            ModelId = modelId,
            TerminologyPaths = terminologyPaths,
            StyleGuidePath = profile?.StyleGuidePath,
            // TODO: Allow settings to override the profile for translation memory enablement.
            UseTranslationMemory = profile?.UseTranslationMemory ?? appSettings.UseTranslationMemory
        };
    }
}
