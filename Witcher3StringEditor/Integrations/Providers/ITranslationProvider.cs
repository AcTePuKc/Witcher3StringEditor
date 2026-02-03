using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Providers;

public interface ITranslationProvider
{
    string Name { get; }

    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TranslationProviderModel>> ListModelsAsync(
        CancellationToken cancellationToken = default);
}
