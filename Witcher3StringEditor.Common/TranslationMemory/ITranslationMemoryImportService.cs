using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.TranslationMemory;

public sealed record TranslationMemoryImportResult(int EntriesImported, int EntriesSkipped);

public interface ITranslationMemoryImportService
{
    Task<TranslationMemoryImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}
