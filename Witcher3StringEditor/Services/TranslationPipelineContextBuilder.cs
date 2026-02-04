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

        var providerName = !string.IsNullOrWhiteSpace(profile?.ProviderName)
            ? profile.ProviderName
            : appSettings.TranslationProviderName;

        var modelName = !string.IsNullOrWhiteSpace(profile?.ModelName)
            ? profile.ModelName
            : appSettings.TranslationModelName;

        return new TranslationPipelineContext
        {
            ProfileId = profile?.Id,
            ProviderName = providerName,
            ModelName = modelName,
            // TODO: Populate terminology/style paths when profile settings are expanded.
            TerminologyPath = null,
            StyleGuidePath = null,
            // TODO: Enable translation memory based on future settings.
            UseTranslationMemory = false
        };
    }
}
