using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationMemoryService : ITranslationMemoryService
{
    public Task<TranslationMemoryEntry?> LookupAsync(
        TranslationMemoryQuery query,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default)
    {
        _ = query;
        _ = context;
        return Task.FromResult<TranslationMemoryEntry?>(null);
    }

    public Task SaveAsync(
        TranslationMemoryEntry entry,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default)
    {
        _ = entry;
        _ = context;
        return Task.CompletedTask;
    }
}
