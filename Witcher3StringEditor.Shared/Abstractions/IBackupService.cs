namespace Witcher3StringEditor.Shared.Abstractions;

public interface IBackupService
{
    public bool Backup(string path);

    public bool Restore(IBackupItem backupItem);

    public bool Delete(IBackupItem backupItem);
}