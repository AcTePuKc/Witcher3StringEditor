using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Storage;

public sealed record TranslationMemoryEntry(
    string SourceText,
    string TargetText,
    string SourceLanguage,
    string TargetLanguage,
    DateTimeOffset CreatedAt,
    string? ProviderName = null,
    string? ModelId = null);

public interface ITranslationMemoryStore
{
    Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default);

    Task UpsertAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TranslationMemoryEntry>> FindExactAsync(
        string sourceText,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TranslationMemoryEntry>> LookupAsync(
        TranslationMemoryLookupRequest request,
        CancellationToken cancellationToken = default);
}
