using JetBrains.Annotations;

namespace Witcher3StringEditor.Common.Abstractions;

public interface IBackupItem
{
    [UsedImplicitly] public string FileName { get; }

    public string Hash { get; }

    public string OrginPath { get; }

    public string BackupPath { get; }

    [UsedImplicitly] public DateTime BackupTime { get; }
}