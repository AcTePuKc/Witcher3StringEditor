using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfileSettingsResolver : ITranslationProfileSettingsResolver
{
    public Task<TranslationProfile?> ResolveAsync(
        IAppSettings settings,
        CancellationToken cancellationToken = default)
    {
        _ = settings;
        // TODO: Merge the selected profile (TranslationProfileId) with settings once profile selection is wired.
        return Task.FromResult<TranslationProfile?>(null);
    }
}
