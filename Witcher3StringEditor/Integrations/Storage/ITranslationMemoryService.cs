using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Storage;

/// <summary>
///     Orchestrates translation memory lookups and persistence for future integrations.
/// </summary>
public interface ITranslationMemoryService
{
    Task<IReadOnlyList<TranslationMemoryEntry>> LookupExactAsync(
        string sourceText,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);

    Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default);
}
