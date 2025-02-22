using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using System.IO;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel(IAppSettings appSettings, IBackupService backupService)
    : ObservableObject, IModalDialogViewModel
{
    public IAppSettings AppSettings => appSettings;
    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Restore(IBackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath))
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupFileNoFound"))
                backupService.Delete(backupItem);
        }
        else
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupRestore")
                && !backupService.Restore(backupItem))
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
        }
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupDelete")
            && !backupService.Delete(backupItem))
            _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
    }
}