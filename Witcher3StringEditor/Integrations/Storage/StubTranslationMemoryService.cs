using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Storage;

public sealed class StubTranslationMemoryService : ITranslationMemoryService
{
    public Task<IReadOnlyList<TranslationMemoryEntry>> LookupExactAsync(
        string sourceText,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        _ = sourceText ?? throw new ArgumentNullException(nameof(sourceText));
        _ = sourceLanguage ?? throw new ArgumentNullException(nameof(sourceLanguage));
        _ = targetLanguage ?? throw new ArgumentNullException(nameof(targetLanguage));

        IReadOnlyList<TranslationMemoryEntry> results = Array.Empty<TranslationMemoryEntry>();
        return Task.FromResult(results);
    }

    public Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default)
    {
        _ = entry ?? throw new ArgumentNullException(nameof(entry));
        return Task.CompletedTask;
    }
}
