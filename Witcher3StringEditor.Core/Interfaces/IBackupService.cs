namespace Witcher3StringEditor.Core.Interfaces;

public interface IBackupService
{
    public void Backup(string path);

    public void Restore(IBackupItem backupItem);

    public void Delete(IBackupItem backupItem);
}