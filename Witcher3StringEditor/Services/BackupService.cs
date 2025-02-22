using CommunityToolkit.Diagnostics;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class BackupService(IAppSettings appSettings) : IBackupService
{
    private readonly string backupFolderPath
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Debugger.IsAttached ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor", "Backup");

    private static string ComputeSha256Hash(string filePath)
    {
        try
        {
            Guard.IsTrue(File.Exists(filePath));
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to compute SHA256 hash: {0}.", filePath);
            return string.Empty;
        }
    }

    public bool Backup(string path)
    {
        try
        {
            Guard.IsTrue(File.Exists(path));
            var hash = ComputeSha256Hash(path);
            Guard.IsNotNullOrWhiteSpace(hash);
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
            Log.Error(ex, "Failed to backup file: {0}.", path);
            return false;
        }
    }

    public bool Restore(IBackupItem backupItem)
    {
        try
        {
            Guard.IsTrue(File.Exists(backupItem.BackupPath));
            var folder = Path.GetDirectoryName(backupItem.OrginPath);
            Guard.IsNotNullOrWhiteSpace(folder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.Copy(backupItem.BackupPath, backupItem.OrginPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore backup item: {0}.", backupItem.OrginPath);
            return false;
        }
    }

    public bool Delete(IBackupItem backupItem)
    {
        try
        {
            if (File.Exists(backupItem.BackupPath))
                File.Delete(backupItem.BackupPath);
            _ = appSettings.BackupItems.Remove(backupItem);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete backup item: {0}.", backupItem.BackupPath);
            return false;
        }
    }
}