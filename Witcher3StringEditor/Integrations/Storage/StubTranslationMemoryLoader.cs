using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Storage;

/// <summary>
///     Stub loader that returns empty results until translation memory import is implemented.
/// </summary>
public sealed class StubTranslationMemoryLoader : ITranslationMemoryLoader
{
    public Task<IReadOnlyList<TranslationMemoryEntry>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Translation memory path is required.", nameof(path));
        }

        // TODO: Parse translation memory files once the import format is approved.
        IReadOnlyList<TranslationMemoryEntry> entries = Array.Empty<TranslationMemoryEntry>();
        return Task.FromResult(entries);
    }
}
