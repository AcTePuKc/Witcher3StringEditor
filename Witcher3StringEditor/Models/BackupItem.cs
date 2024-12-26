using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models;

internal record BackupItem : IBackupItem
{
    public required string FileName { get; init; }

    public required string Hash { get; init; }

    public required string OrginPath { get; init; }

    public required string BackupPath { get; init; }

    public required DateTime BackupTime { get; init; }
}