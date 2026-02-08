using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

public sealed class StubTranslationProfileStore : ITranslationProfileStore
{
    public Task<IReadOnlyList<TranslationProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TranslationProfile> profiles = Array.Empty<TranslationProfile>();
        return Task.FromResult(profiles);
    }

    public Task SaveAsync(TranslationProfile profile, CancellationToken cancellationToken = default)
    {
        _ = profile ?? throw new ArgumentNullException(nameof(profile));
        return Task.CompletedTask;
    }
}
