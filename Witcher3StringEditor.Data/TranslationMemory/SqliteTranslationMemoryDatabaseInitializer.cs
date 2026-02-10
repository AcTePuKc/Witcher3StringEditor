using System;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.TranslationMemory;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.TranslationMemory;

public sealed class SqliteTranslationMemoryDatabaseInitializer : ITranslationMemoryDatabaseInitializer
{
    public Task InitializeAsync(TranslationMemorySettings settings, CancellationToken cancellationToken = default)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var bootstrap = new SqliteBootstrap(settings.DatabasePath);
        return bootstrap.InitializeAsync(cancellationToken);
    }
}
