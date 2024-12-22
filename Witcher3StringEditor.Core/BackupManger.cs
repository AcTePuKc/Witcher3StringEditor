using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core;

public class BackupManger
{
    private readonly string backupPath;

    private readonly string jsonPath;

    private static readonly Lazy<BackupManger> LazyInstance
        = new(static () => new BackupManger(".\\Backup"));

    public static BackupManger Instance => LazyInstance.Value;

    public readonly ObservableCollection<IBackupItem> BackupItems;

    private BackupManger(string backupPath)
    {
        this.backupPath = backupPath;
        jsonPath = $"{backupPath}\\History.json";
        BackupItems = new ObservableCollection<IBackupItem>(GetHistoryItems(jsonPath));
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
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
        BackupItems.Add(backupItem);
        UpdateHistoryItems(BackupItems, jsonPath);
    }

    public static void Restore(IBackupItem backupItem)
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
        BackupItems.Remove(backupItem);
        UpdateHistoryItems(BackupItems, jsonPath);
    }

    private static void UpdateHistoryItems(IEnumerable<IBackupItem> backups, string path)
        => File.WriteAllText(path, JsonConvert.SerializeObject(backups));

    private static IEnumerable<IBackupItem> GetHistoryItems(string path)
    {
        if (!File.Exists(path)) return [];
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<IEnumerable<BackupItem>>(json) ?? [];
    }
}