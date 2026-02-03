using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfileStore : ITranslationProfileStore
{
    public Task<IReadOnlyList<TranslationProfile>> ListAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with JSON or SQLite-backed profile storage.
        IReadOnlyList<TranslationProfile> profiles = [];
        return Task.FromResult(profiles);
    }

    public Task<TranslationProfile?> GetAsync(string profileId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement profile lookup by id.
        return Task.FromResult<TranslationProfile?>(null);
    }
}
