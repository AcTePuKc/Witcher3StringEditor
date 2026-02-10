using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

public interface ITranslationProfileSummaryCatalog
{
    Task<IReadOnlyList<TranslationProfileSummary>> ListSummariesAsync(
        CancellationToken cancellationToken = default);
}
