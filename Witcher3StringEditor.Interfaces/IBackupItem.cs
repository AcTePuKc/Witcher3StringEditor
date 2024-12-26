namespace Witcher3StringEditor.Interfaces;

public interface IBackupItem
{
    public string FileName { get; init; }

    public string Hash { get; init; }

    public string OrginPath { get; init; }

    public string BackupPath { get; init; }

    public DateTime BackupTime { get; init; }
}