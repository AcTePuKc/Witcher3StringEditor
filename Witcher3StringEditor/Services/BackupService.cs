using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class BackupService(IAppSettings appSettings) : IBackupService
{
    private readonly string backBasePath 
        = Path.Combine(Environment.ExpandEnvironmentVariables("%appdata%"), "Witcher3StringEditor", "Backup");

    private static string ComputeSha256Hash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    public void Backup(string path)
    {
        var backupItem = new BackupItem
        {
            FileName = Path.GetFileName(path),
            Hash = ComputeSha256Hash(path),
            OrginPath = path,
            BackupPath = Path.Combine(backBasePath, $"{Guid.NewGuid():N}.bak"),
            BackupTime = DateTime.Now
        };
        if (!Directory.Exists(backBasePath))
            Directory.CreateDirectory(backBasePath);
        if (appSettings.BackupItems.Any(x => x.Hash == backupItem.Hash && x.OrginPath == backupItem.OrginPath)) return;
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
        appSettings.BackupItems.Add(backupItem);
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