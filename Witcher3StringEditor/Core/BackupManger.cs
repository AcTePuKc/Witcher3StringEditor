using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Core;

public static class BackupManger
{
    private const string HistoryPath = ".\\Backup\\History.json";
    public static ObservableCollection<BackupItem> BackupItems { get; } = new(GetHistoryItems());

    private static string ComputeSha256Hash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static void Backup(string path)
    {
        var backupItem = new BackupItem
        {
            FileName = Path.GetFileName(path),
            Hash = ComputeSha256Hash(path),
            OrginPath = path,
            BackupPath = Path.Combine($".\\Backup\\{Guid.NewGuid()}"),
            BackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        BackupItems.Add(backupItem);
        UpdateHistoryItems(BackupItems);

        if (!Directory.Exists("Backup"))
            Directory.CreateDirectory("Backup");
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
    }

    public static void Restore(BackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath)) return;
        var folder = Path.GetDirectoryName(backupItem.OrginPath);
        if (folder == null) return;
        if (!Directory.Exists(folder))
            _ = Directory.CreateDirectory(folder);
        File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
    }

    public static void Delete(BackupItem backupItem)
    {
        if (File.Exists(backupItem.BackupPath))
            File.Delete(backupItem.BackupPath);
        if (BackupItems.Remove(backupItem))
            UpdateHistoryItems(BackupItems);
    }

    private static void UpdateHistoryItems(IEnumerable<BackupItem> backups)
    {
        var json = JsonConvert.SerializeObject(backups);
        File.WriteAllText(HistoryPath, json);
    }

    private static IEnumerable<BackupItem> GetHistoryItems()
    {
        if (!File.Exists(HistoryPath)) return [];
        var json = File.ReadAllText(HistoryPath);
        return JsonConvert.DeserializeObject<IEnumerable<BackupItem>>(json) ?? [];
    }
}