using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using Serilog;
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
            Log.Error("The backup file {0} does no exist.", backupItem.BackupPath);
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupFileNoFound"))
            {
                _ = backupService.Delete(backupItem);
                Log.Information("The backup item for file {0} has been deleted.", backupItem.BackupPath);
            }
        }
        else
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupRestore"))
            {
                Log.Information("The restoration of file {0} has been approved.", backupItem.OrginPath);
                if (!backupService.Restore(backupItem))
                {
                    _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
                    Log.Error("The restoration of file {0} has failed.", backupItem.OrginPath);
                }
            }
        }
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BackupDelete"))
        {
            Log.Information("The deletion of file {0} has been approved.", backupItem.BackupPath);
            if (!backupService.Delete(backupItem))
            {
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "OperationFailed");
                Log.Error("The deletion of file {0} has failed.", backupItem.BackupPath);
            }
        }
    }
}