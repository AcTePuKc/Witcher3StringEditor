using System;
using Witcher3StringEditor.Common.TranslationMemory;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.TranslationMemory;

/// <summary>
///     Creates SQLite-backed translation memory stores from runtime settings.
/// </summary>
public sealed class SqliteTranslationMemoryStoreFactory : ITranslationMemoryStoreFactory
{
    public ITranslationMemoryStore Create(TranslationMemorySettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        // TODO: Validate settings and inject configuration once TM wiring is approved.
        var bootstrap = new SqliteBootstrap(settings.DatabasePath);
        return new SqliteTranslationMemoryStore(bootstrap);
    }
}
