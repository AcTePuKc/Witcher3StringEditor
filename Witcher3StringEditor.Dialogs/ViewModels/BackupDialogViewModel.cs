using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.IO;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel(IBackupService backupService, IAppSettings appSettings) 
    : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public IAppSettings AppSettings => appSettings;

    [RelayCommand]
    private async Task Restore(IBackupItem backupItem)
    {
        if (!File.Exists(backupItem.BackupPath) && await WeakReferenceMessenger.Default.Send(new BackupMessage(), "BackupFileNoFound"))
        {
            backupService.Delete(backupItem); return;
        }

        if (await WeakReferenceMessenger.Default.Send(new BackupMessage(), "BackupRestore"))
            backupService.Restore(backupItem);
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new BackupMessage(), "BackupDelete"))
            backupService.Delete(backupItem);
    }
}