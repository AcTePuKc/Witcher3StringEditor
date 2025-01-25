using Serilog;
using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class BackupService(IAppSettings appSettings) : IBackupService
{
    private readonly string backupFolderPath
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Witcher3StringEditor", "Backup");

    private static string ComputeSha256Hash(string filePath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to compute SHA256 hash.");
            return string.Empty;
        }
    }

    public bool Backup(string path)
    {
        try
        {
            var hash = ComputeSha256Hash(path);
            if (string.IsNullOrWhiteSpace(hash)) return false;
            var backupItem = new BackupItem
            {
                FileName = Path.GetFileName(path),
                Hash = hash,
                OrginPath = path,
                BackupPath = Path.Combine(backupFolderPath, $"{Guid.NewGuid():N}.bak"),
                BackupTime = DateTime.Now
            };
            if (!Directory.Exists(backupFolderPath))
                Directory.CreateDirectory(backupFolderPath);
            if (appSettings.BackupItems.Any(x => x.Hash == backupItem.Hash && x.OrginPath == backupItem.OrginPath && File.Exists(x.BackupPath))) return true;
            File.Copy(backupItem.OrginPath, backupItem.BackupPath);
            appSettings.BackupItems.Add(backupItem);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to backup file {Path}", path);
            return false;
        }
    }

    public bool Restore(IBackupItem backupItem)
    {
        try
        {
            if (!File.Exists(backupItem.BackupPath)) return false;
            var folder = Path.GetDirectoryName(backupItem.OrginPath);
            if (folder == null) return false;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore backup item: {OrginPath}.", backupItem.OrginPath);
            return false;
        }
    }

    public bool Delete(IBackupItem backupItem)
    {
        try
        {
            if (File.Exists(backupItem.BackupPath))
                File.Delete(backupItem.BackupPath);
            appSettings.BackupItems.Remove(backupItem);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete backup item: {BackupPath}", backupItem.BackupPath);
            return false;
        }
    }
}