using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.TranslationMemory;

public interface ITranslationMemoryStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<TranslationMemoryEntry?> FindAsync(TranslationMemoryQuery query, CancellationToken cancellationToken = default);

    Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default);
}
