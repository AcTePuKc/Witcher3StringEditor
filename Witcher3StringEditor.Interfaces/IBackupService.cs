namespace Witcher3StringEditor.Interfaces;

public interface IBackupService
{
    public bool Backup(string path);

    public bool Restore(IBackupItem backupItem);

    public bool Delete(IBackupItem backupItem);
}