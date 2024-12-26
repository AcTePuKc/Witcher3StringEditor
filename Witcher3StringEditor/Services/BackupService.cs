using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class BackupService : IBackupService
{
    private readonly string backupPath;

    private readonly string jsonPath;

    private static readonly Lazy<BackupService> LazyInstance
        = new(static () => new BackupService(".\\Backup"));

    private readonly List<IBackupItem> backupItems;

    public static BackupService Instance => LazyInstance.Value;

    private BackupService(string path)
    {
        backupPath = path;
        jsonPath = $"{path}\\History.json";
        backupItems = GetAllBackup().ToList();
    }

    private static string ComputeSha256Hash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public void Backup(string path)
    {
        var backupItem = new BackupItem
        {
            FileName = Path.GetFileName(path),
            Hash = ComputeSha256Hash(path),
            OrginPath = path,
            BackupPath = Path.Combine($"{backupPath}\\{Guid.NewGuid():N}.bak"),
            BackupTime = DateTime.Now
        };

        if (!Directory.Exists(backupPath))
            Directory.CreateDirectory(backupPath);
        if (backupItems.Any(x => x.Hash == backupItem.Hash && x.OrginPath == backupItem.OrginPath)) return;
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
        UpdateBackupRecords(backupItems.Append(backupItem));
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
        backupItems.Remove(backupItem);
        UpdateBackupRecords(backupItems);
    }

    private void UpdateBackupRecords(IEnumerable<IBackupItem> backups)
    {
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(backups));
    }

    public IEnumerable<IBackupItem> GetAllBackup()
    {
        if (!File.Exists(jsonPath)) return [];
        var json = File.ReadAllText(jsonPath);
        return JsonConvert.DeserializeObject<IEnumerable<BackupItem>>(json) ?? [];
    }
}