using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Witcher3StringEditor.Data.Storage;

public sealed class SqliteBootstrap
{
    private readonly string databasePath;

    public SqliteBootstrap(string? databasePath = null)
    {
        this.databasePath = string.IsNullOrWhiteSpace(databasePath)
            ? AppDataPathProvider.GetDatabasePath()
            : databasePath;
    }

    public string DatabasePath => databasePath;

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={databasePath}");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await CreateTablesAsync(connection, cancellationToken);
    }

    private static async Task CreateTablesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS TranslationMemory (
    SourceText TEXT NOT NULL,
    TargetText TEXT NOT NULL,
    SourceLanguage TEXT NOT NULL,
    TargetLanguage TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    PRIMARY KEY (SourceText, SourceLanguage, TargetLanguage)
);

CREATE TABLE IF NOT EXISTS QualityAssurance (
    SourceText TEXT NOT NULL,
    TargetText TEXT NOT NULL,
    IssueType TEXT NOT NULL,
    Details TEXT,
    CreatedAt TEXT NOT NULL,
    PRIMARY KEY (SourceText, TargetText, IssueType)
);
";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
