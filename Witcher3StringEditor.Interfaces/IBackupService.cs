namespace Witcher3StringEditor.Interfaces;

public interface IBackupService
{
    public void Backup(string path);

    public void Restore(IBackupItem backupItem);

    public void Delete(IBackupItem backupItem);
}