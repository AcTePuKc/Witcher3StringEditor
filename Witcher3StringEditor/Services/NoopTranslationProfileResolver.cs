using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfileResolver : ITranslationProfileResolver
{
    public Task<TranslationProfile?> ResolveAsync(string? profileId, CancellationToken cancellationToken = default)
    {
        // TODO: Resolve profiles from the profile store and merge into app settings defaults.
        _ = profileId;
        return Task.FromResult<TranslationProfile?>(null);
    }
}
