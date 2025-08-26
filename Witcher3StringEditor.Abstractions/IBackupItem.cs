namespace Witcher3StringEditor.Abstractions;

public interface IBackupItem
{
    public string FileName { get; }

    public string Hash { get; }

    public string OrginPath { get; }

    public string BackupPath { get; }

    public DateTime BackupTime { get; }
}