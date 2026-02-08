using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationMemoryImportService : ITranslationMemoryImportService
{
    public Task<TranslationMemoryImportResult> ImportAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        // TODO: Replace with real translation memory import logic (CSV/JSON/SQLite).
        return Task.FromResult(new TranslationMemoryImportResult(0, 0));
    }
}
