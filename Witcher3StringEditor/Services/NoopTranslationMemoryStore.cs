using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationMemoryStore : ITranslationMemoryStore
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with SQLite-backed translation memory initialization.
        return Task.CompletedTask;
    }

    public Task<TranslationMemoryEntry?> FindAsync(TranslationMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement lookup once schema and matching strategy are defined.
        return Task.FromResult<TranslationMemoryEntry?>(null);
    }

    public Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default)
    {
        // TODO: Implement persistence to local storage.
        return Task.CompletedTask;
    }
}
