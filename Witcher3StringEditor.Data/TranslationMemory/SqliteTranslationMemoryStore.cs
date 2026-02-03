using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Witcher3StringEditor.Common.TranslationMemory;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.TranslationMemory;

public sealed class SqliteTranslationMemoryStore : ITranslationMemoryStore
{
    private readonly SqliteBootstrap bootstrap;

    public SqliteTranslationMemoryStore(SqliteBootstrap bootstrap)
    {
        this.bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return bootstrap.InitializeAsync(cancellationToken);
    }

    public async Task<TranslationMemoryEntry?> FindAsync(
        TranslationMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var connection = bootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT SourceText, TargetText, SourceLanguage, TargetLanguage, CreatedAt
FROM TranslationMemory
WHERE SourceText = $sourceText
  AND SourceLanguage = $sourceLanguage
  AND TargetLanguage = $targetLanguage
LIMIT 1;
";
        command.Parameters.AddWithValue("$sourceText", query.SourceText);
        command.Parameters.AddWithValue("$sourceLanguage", NormalizeLanguage(query.SourceLanguage));
        command.Parameters.AddWithValue("$targetLanguage", NormalizeLanguage(query.TargetLanguage));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new TranslationMemoryEntry
        {
            SourceText = reader.GetString(0),
            TargetText = reader.GetString(1),
            SourceLanguage = DenormalizeLanguage(reader.GetString(2)),
            TargetLanguage = DenormalizeLanguage(reader.GetString(3)),
            CreatedAt = DateTimeOffset.Parse(reader.GetString(4))
        };
    }

    public async Task SaveAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await using var connection = bootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO TranslationMemory (SourceText, TargetText, SourceLanguage, TargetLanguage, CreatedAt)
VALUES ($sourceText, $targetText, $sourceLanguage, $targetLanguage, $createdAt)
ON CONFLICT (SourceText, SourceLanguage, TargetLanguage)
DO UPDATE SET
    TargetText = excluded.TargetText,
    CreatedAt = excluded.CreatedAt;
";
        command.Parameters.AddWithValue("$sourceText", entry.SourceText);
        command.Parameters.AddWithValue("$targetText", entry.TargetText);
        command.Parameters.AddWithValue("$sourceLanguage", NormalizeLanguage(entry.SourceLanguage));
        command.Parameters.AddWithValue("$targetLanguage", NormalizeLanguage(entry.TargetLanguage));
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string NormalizeLanguage(string? language)
    {
        return language ?? string.Empty;
    }

    private static string? DenormalizeLanguage(string language)
    {
        return string.IsNullOrWhiteSpace(language) ? null : language;
    }
}
