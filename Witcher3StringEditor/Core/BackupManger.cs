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
        if (!Directory.Exists("Backup"))
            Directory.CreateDirectory("Backup");

        var backupItem = new BackupItem
        {
            FileName = Path.GetFileName(path),
            Hash = ComputeSha256Hash(path),
            OrginPath = path,
            BackupPath = Path.Combine($".\\Backup\\{Guid.NewGuid()}"),
            BackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        BackupItems.Add(backupItem);

        File.Copy(backupItem.OrginPath, backupItem.BackupPath);

        UpdateHistoryItems();
    }

    public static void Restore(BackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath)) return;
        var folder = Path.GetDirectoryName(backupItem.OrginPath);
        if (folder == null) return;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
    }

    public static void Delete(BackupItem backupItem)
    {
        if (File.Exists(backupItem.BackupPath))
        {
            File.Delete(backupItem.BackupPath);
            BackupItems.Remove(backupItem);
        }

        UpdateHistoryItems();
    }

    private static void UpdateHistoryItems()
    {
        var json = JsonConvert.SerializeObject(BackupItems);
        File.WriteAllText(HistoryPath, json);
    }

    private static IEnumerable<BackupItem> GetHistoryItems()
    {
        if (!File.Exists(HistoryPath)) return [];
        var json = File.ReadAllText(HistoryPath);
        var items = JsonConvert.DeserializeObject<IEnumerable<BackupItem>>(json);
        return items ?? [];
    }
}