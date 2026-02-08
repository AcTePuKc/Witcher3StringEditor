using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfileCatalog : ITranslationProfileCatalog
{
    public Task<IReadOnlyList<TranslationProfileSummary>> ListSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TranslationProfileSummary> profiles = [];
        return Task.FromResult(profiles);
    }
}
