namespace Witcher3StringEditor.Models;

public record BackupItem
{
    public required string FileName { get; init; }
    public required string Hash { get; init; }

    public required string OrginPath { get; init; }

    public required string BackupPath { get; init; }

    public required string BackupTime { get; init; }
}