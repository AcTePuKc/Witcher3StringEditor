using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Compile-safe placeholder until local JSON persistence is wired.
/// </summary>
internal sealed class NoopTranslationModelSelectionStore : ITranslationModelSelectionStore
{
    public Task<TranslationModelSelection?> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TranslationModelSelection?>(null);
    }

    public Task SaveAsync(TranslationModelSelection selection, CancellationToken cancellationToken = default)
    {
        // TODO: Persist to local JSON settings store in a future issue.
        return Task.CompletedTask;
    }
}
