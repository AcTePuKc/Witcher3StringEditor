using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Data.Profiles;

public sealed class TranslationProfileCatalog : ITranslationProfileCatalog
{
    private readonly ITranslationProfileStore store;

    public TranslationProfileCatalog(ITranslationProfileStore store)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<IReadOnlyList<TranslationProfileSummary>> ListSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        var profiles = await store.ListAsync(cancellationToken);

        return profiles
            .Select(profile => new TranslationProfileSummary(profile.Id, profile.Name, false))
            .ToList();
    }
}
