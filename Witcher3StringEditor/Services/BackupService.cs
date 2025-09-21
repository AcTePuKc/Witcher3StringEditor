using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides backup functionality for files
///     Implements the IBackupService interface to handle creating, restoring, and deleting file backups
/// </summary>
internal class BackupService(IAppSettings appSettings) : IBackupService
{
    /// <summary>
    ///     The path to the backup folder where backup files are stored
    /// </summary>
    private readonly string backupFolderPath
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor", "Backup");

    /// <summary>
    ///     Gets a value indicating whether the application is running in debug mode
    /// </summary>
    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    /// <summary>
    ///     Creates a backup of the specified file
    /// </summary>
    /// <param name="filePath">The path to the file to back up</param>
    /// <returns>True if the backup was created successfully, false otherwise</returns>
    public bool Backup(string filePath)
    {
        try
        {
            var hash = ValidateAndGetHash(filePath);
            var backupItem = new BackupItem
            {
                FileName = Path.GetFileName(filePath),
                Hash = hash,
                OrginPath = filePath,
                BackupPath = Path.Combine(backupFolderPath, $"{Guid.NewGuid():N}.bak"),
                BackupTime = DateTime.Now
            };
            EnsureBackupDirectoryExists(backupFolderPath);
            return IsDuplicateBackup(backupItem) || ExecuteBackup(backupItem);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to backup file: {Path}.", filePath);
            return false;
        }
    }

    /// <summary>
    ///     Restores a file from the specified backup item
    /// </summary>
    /// <param name="backupItem">The backup item containing information about the backup to restore</param>
    /// <returns>True if the restore operation was successful, false otherwise</returns>
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
            Log.Information("Restore backup file: {FileName}.", backupItem.OrginPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore backup item: {Path}.", backupItem.OrginPath);
            return false;
        }
    }

    /// <summary>
    ///     Deletes the specified backup item
    /// </summary>
    /// <param name="backupItem">The backup item to delete</param>
    /// <returns>True if the deletion was successful, false otherwise</returns>
    public bool Delete(IBackupItem backupItem)
    {
        try
        {
            if (File.Exists(backupItem.BackupPath))
                File.Delete(backupItem.BackupPath);
            appSettings.BackupItems.Remove(backupItem);
            Log.Information("Delete backup file: {Path}.", backupItem.BackupPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete backup item: {Path}.", backupItem.BackupPath);
            return false;
        }
    }

    /// <summary>
    ///     Validates that the file exists and computes its SHA256 hash
    /// </summary>
    /// <param name="filePath">The path to the file to validate and hash</param>
    /// <returns>The SHA256 hash of the file</returns>
    private static string ValidateAndGetHash(string filePath)
    {
        Guard.IsTrue(File.Exists(filePath));
        var hash = ComputeSha256Hash(filePath);
        Guard.IsNotNullOrWhiteSpace(hash);
        return hash;
    }

    /// <summary>
    ///     Ensures that the backup directory exists, creating it if necessary
    /// </summary>
    /// <param name="backupDirectoryPath">The path to the backup directory</param>
    private static void EnsureBackupDirectoryExists(string backupDirectoryPath)
    {
        if (!Directory.Exists(backupDirectoryPath))
            Directory.CreateDirectory(backupDirectoryPath);
    }

    /// <summary>
    ///     Checks if a backup with the same hash and original path already exists
    /// </summary>
    /// <param name="backupItem">The backup item to check for duplicates</param>
    /// <returns>True if a duplicate backup exists, false otherwise</returns>
    private bool IsDuplicateBackup(BackupItem backupItem)
    {
        return appSettings.BackupItems.Any(x =>
            x.Hash == backupItem.Hash &&
            x.OrginPath == backupItem.OrginPath &&
            File.Exists(x.BackupPath));
    }

    /// <summary>
    ///     Executes the backup operation by copying the file to the backup location and adding it to the backup items
    ///     collection
    /// </summary>
    /// <param name="backupItem">The backup item to execute the backup for</param>
    /// <returns>True if the backup was executed successfully</returns>
    private bool ExecuteBackup(BackupItem backupItem)
    {
        File.Copy(backupItem.OrginPath, backupItem.BackupPath);
        appSettings.BackupItems.Add(backupItem);
        Log.Information("Backup file: {Path}.", backupItem.OrginPath);
        return true;
    }

    /// <summary>
    ///     Computes the SHA256 hash of the specified file
    /// </summary>
    /// <param name="filePath">The path to the file to hash</param>
    /// <returns>The SHA256 hash of the file as a hexadecimal string</returns>
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
            Log.Error(ex, "Failed to compute SHA256 hash: {Path}.", filePath);
            return string.Empty;
        }
    }
}