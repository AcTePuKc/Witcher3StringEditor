using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Common.TranslationMemory;

/// <summary>
///     Coordinates translation memory lookups/saves with pipeline context decisions.
/// </summary>
public interface ITranslationMemoryService
{
    Task<TranslationMemoryEntry?> LookupAsync(
        TranslationMemoryQuery query,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        TranslationMemoryEntry entry,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default);
}
