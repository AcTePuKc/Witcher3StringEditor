using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationModelCatalog : ITranslationModelCatalog
{
    public Task<IReadOnlyList<ModelInfo>> GetAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ModelInfo> models = [];
        return Task.FromResult(models);
    }
}
