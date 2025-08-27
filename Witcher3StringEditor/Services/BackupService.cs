using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Services;

internal class BackupService(ObservableCollection<IBackupItem> backupItems, ILogger<IBackupService> logger) : IBackupService
{
    private readonly string backupFolderPath
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor", "Backup");

    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

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
            if (backupItems.Any(x =>
                    x.Hash == backupItem.Hash && x.OrginPath == backupItem.OrginPath && File.Exists(x.BackupPath)))
                return true;
            File.Copy(backupItem.OrginPath, backupItem.BackupPath);
            backupItems.Add(backupItem);
            logger.LogInformation("Backup file: {FileName}.", backupItem.FileName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to backup file: {Path}.", path);
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
            logger.LogInformation("Restore backup file: {FileName}.", backupItem.FileName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to restore backup item: {Path}.", backupItem.OrginPath);
            return false;
        }
    }

    public bool Delete(IBackupItem backupItem)
    {
        try
        {
            if (File.Exists(backupItem.BackupPath))
                File.Delete(backupItem.BackupPath);
            _ = backupItems.Remove(backupItem);
            logger.LogInformation("Delete backup file: {FileName}.", backupItem.FileName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete backup item: {Path}.", backupItem.BackupPath);
            return false;
        }
    }

    private string ComputeSha256Hash(string filePath)
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
            logger.LogError(ex, "Failed to compute SHA256 hash: {Path}.", filePath);
            return string.Empty;
        }
    }
}