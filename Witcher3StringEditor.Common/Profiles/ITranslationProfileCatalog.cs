using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfileCatalog
{
    Task<IReadOnlyList<TranslationProfileSummary>> ListSummariesAsync(
        CancellationToken cancellationToken = default);
}
