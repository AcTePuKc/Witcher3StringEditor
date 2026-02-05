using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationModelCatalog
{
    Task<IReadOnlyList<ModelInfo>> GetAsync(
        string providerName,
        CancellationToken cancellationToken = default);
}
