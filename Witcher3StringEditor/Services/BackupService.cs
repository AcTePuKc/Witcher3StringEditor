using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class BackupService(IAppSettings appSettings) : IBackupService
{
    public readonly IAppSettings appSettings = appSettings;

    private static string ComputeSha256Hash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public IBackupItem? Backup(string path)
    {
        var backupItem = new BackupItem
        {
            FileName = Path.GetFileName(path),
            Hash = ComputeSha256Hash(path),
            OrginPath = path,
            BackupPath = Path.Combine($".\\Data\\{Guid.NewGuid():N}.bak"),
            BackupTime = DateTime.Now
        };

        if (!Directory.Exists(".\\Data"))
            Directory.CreateDirectory(".\\Data");
        if (appSettings.BackupItems.Any(x => x.Hash == backupItem.Hash && x.OrginPath == backupItem.OrginPath)) return null;
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
        return backupItem;
    }

    public void Restore(IBackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath)) return;
        var folder = Path.GetDirectoryName(backupItem.OrginPath);
        if (folder == null) return;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
    }

    public void Delete(IBackupItem backupItem)
    {
        if (File.Exists(backupItem.BackupPath))
            File.Delete(backupItem.BackupPath);
        appSettings.BackupItems.Remove(backupItem);
    }
}