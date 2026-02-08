using System;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.TranslationMemory;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.TranslationMemory;

public sealed class SqliteTranslationMemoryDatabaseInitializer : ITranslationMemoryDatabaseInitializer
{
    private readonly SqliteBootstrap bootstrap;

    public SqliteTranslationMemoryDatabaseInitializer(SqliteBootstrap bootstrap)
    {
        this.bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
    }

    public Task InitializeAsync(TranslationMemorySettings settings, CancellationToken cancellationToken = default)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        // TODO: Allow the database path to be overridden by settings when wiring TM configuration.
        return bootstrap.InitializeAsync(cancellationToken);
    }
}
