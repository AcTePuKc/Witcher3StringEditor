using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel(
    IAppSettings appSettings,
    IBackupService backupService,
    ILogger<BackupDialogViewModel> logger)
    : ObservableObject, IModalDialogViewModel
{
    public IAppSettings AppSettings => appSettings;
    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Restore(IBackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath))
        {
            logger.LogError("The backup file {Path} does no exist.", backupItem.BackupPath);
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupFileNoFound"))
                _ = backupService.Delete(backupItem);
        }
        else
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupRestore"))
            {
                logger.LogInformation("The restoration of file {Path} has been approved.", backupItem.OrginPath);
                if (!backupService.Restore(backupItem))
                    _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
            }
        }
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupDelete"))
        {
            logger.LogInformation("The deletion of file {Path} has been approved.", backupItem.BackupPath);
            if (!backupService.Delete(backupItem))
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
        }
    }
}