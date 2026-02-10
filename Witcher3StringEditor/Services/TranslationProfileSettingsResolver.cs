using System;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationProfileSettingsResolver : ITranslationProfileSettingsResolver
{
    private readonly ITranslationProfileResolver profileResolver;

    public TranslationProfileSettingsResolver(ITranslationProfileResolver profileResolver)
    {
        this.profileResolver = profileResolver;
    }

    public async Task<TranslationProfile?> ResolveAsync(
        IAppSettings settings,
        CancellationToken cancellationToken = default)
    {
        _ = settings ?? throw new ArgumentNullException(nameof(settings));

        var profileId = settings.TranslationProfileId;
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        var profile = await profileResolver.ResolveAsync(profileId, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return null;
        }

        return MergeProfile(settings, profile);
    }

    private static TranslationProfile MergeProfile(IAppSettings settings, TranslationProfile profile)
    {
        return new TranslationProfile
        {
            Id = profile.Id,
            Name = ResolveString(profile.Name, profile.Id),
            ProviderName = ResolveString(profile.ProviderName, settings.TranslationProviderName),
            ModelName = ResolveString(profile.ModelName, settings.TranslationModelName),
            BaseUrl = ResolveString(profile.BaseUrl, settings.TranslationBaseUrl),
            TerminologyPath = ResolveOptionalPath(profile.TerminologyPath),
            TerminologyFilePath = ResolveOptionalPath(
                profile.TerminologyFilePath,
                profile.TerminologyPath,
                settings.TerminologyFilePath),
            StyleGuidePath = ResolveOptionalPath(profile.StyleGuidePath),
            StyleGuideFilePath = ResolveOptionalPath(
                profile.StyleGuideFilePath,
                profile.StyleGuidePath,
                settings.StyleGuideFilePath),
            UseTerminologyPack = profile.UseTerminologyPack ?? settings.UseTerminologyPack,
            UseStyleGuide = profile.UseStyleGuide ?? settings.UseStyleGuide,
            UseTranslationMemory = profile.UseTranslationMemory ?? settings.UseTranslationMemory,
            Notes = profile.Notes
        };
    }

    private static string ResolveString(string? profileValue, string? fallbackValue)
    {
        if (!string.IsNullOrWhiteSpace(profileValue))
        {
            return profileValue.Trim();
        }

        return string.IsNullOrWhiteSpace(fallbackValue) ? string.Empty : fallbackValue.Trim();
    }

    private static string? ResolveOptionalPath(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate.Trim();
            }
        }

        return null;
    }
}
