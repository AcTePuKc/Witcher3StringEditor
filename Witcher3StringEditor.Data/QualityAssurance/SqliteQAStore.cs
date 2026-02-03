using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Witcher3StringEditor.Common.QualityAssurance;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.QualityAssurance;

public sealed class SqliteQAStore : IQAStore
{
    private readonly SqliteBootstrap bootstrap;

    public SqliteQAStore(SqliteBootstrap bootstrap)
    {
        this.bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return bootstrap.InitializeAsync(cancellationToken);
    }

    public async Task<QualityAssuranceEntry?> FindAsync(
        QualityAssuranceQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var connection = bootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT SourceText, TargetText, IssueType, Details, CreatedAt
FROM QualityAssurance
WHERE SourceText = $sourceText
  AND TargetText = $targetText
  AND IssueType = $issueType
LIMIT 1;
";
        command.Parameters.AddWithValue("$sourceText", query.SourceText);
        command.Parameters.AddWithValue("$targetText", query.TargetText);
        command.Parameters.AddWithValue("$issueType", query.IssueType);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var sourceTextIndex = reader.GetOrdinal("SourceText");
        var targetTextIndex = reader.GetOrdinal("TargetText");
        var issueTypeIndex = reader.GetOrdinal("IssueType");
        var detailsIndex = reader.GetOrdinal("Details");
        var createdAtIndex = reader.GetOrdinal("CreatedAt");

        return new QualityAssuranceEntry
        {
            SourceText = reader.GetString(sourceTextIndex),
            TargetText = reader.GetString(targetTextIndex),
            IssueType = reader.GetString(issueTypeIndex),
            Details = reader.IsDBNull(detailsIndex) ? null : reader.GetString(detailsIndex),
            CreatedAt = reader.GetDateTimeOffset(createdAtIndex)
        };
    }

    public async Task SaveAsync(QualityAssuranceEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await using var connection = bootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO QualityAssurance (SourceText, TargetText, IssueType, Details, CreatedAt)
VALUES ($sourceText, $targetText, $issueType, $details, $createdAt)
ON CONFLICT (SourceText, TargetText, IssueType)
DO UPDATE SET
    Details = excluded.Details,
    CreatedAt = excluded.CreatedAt;
";
        command.Parameters.AddWithValue("$sourceText", entry.SourceText);
        command.Parameters.AddWithValue("$targetText", entry.TargetText);
        command.Parameters.AddWithValue("$issueType", entry.IssueType);
        command.Parameters.AddWithValue("$details", (object?)entry.Details ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
