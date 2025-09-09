using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel(
    IAppSettings appSettings,
    IBackupService backupService)
    : ObservableObject, IModalDialogViewModel
{
    public IAppSettings AppSettings => appSettings;
    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Restore(IBackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath))
            await HandleMissingBackupFile(backupItem);
        else
            await HandleExistingBackupFile(backupItem);
    }

    private async Task HandleExistingBackupFile(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupRestore"))
        {
            Log.Information("The restoration of file {Path} has been approved.", backupItem.OrginPath);
            if (!backupService.Restore(backupItem))
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
        }
    }

    private async Task HandleMissingBackupFile(IBackupItem backupItem)
    {
        Log.Error("The backup file {Path} does no exist.", backupItem.BackupPath);
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupFileNoFound"))
            _ = backupService.Delete(backupItem);
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupDelete"))
        {
            Log.Information("The deletion of file {Path} has been approved.", backupItem.BackupPath);
            if (!backupService.Delete(backupItem))
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
        }
    }
}