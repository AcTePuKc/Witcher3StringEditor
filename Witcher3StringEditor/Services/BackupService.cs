using Serilog;
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
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to compute SHA256 hash: {ex.Message}");
            throw;
        }
    }

    public void Backup(string path)
    {
        try
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
        catch (Exception ex)
        {
            Log.Error($"Failed to backup file '{path}': {ex.Message}");
        }
    }

    public void Restore(IBackupItem backupItem)
    {
        try
        {
            if (!File.Exists(backupItem.BackupPath)) return;
            var folder = Path.GetDirectoryName(backupItem.OrginPath);
            if (folder == null) return;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to restore backup item '{backupItem.OrginPath}': {ex.Message}");
        }
    }

    public void Delete(IBackupItem backupItem)
    {
        try
        {
            if (File.Exists(backupItem.BackupPath))
                File.Delete(backupItem.BackupPath);
            appSettings.BackupItems.Remove(backupItem);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to delete backup item '{backupItem.BackupPath}': {ex.Message}");
        }
    }
}