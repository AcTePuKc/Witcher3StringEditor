using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using System.IO;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel(IBackupService backupService) : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public ObservableCollection<IBackupItem> BackupItems { get; }
        = new(backupService.GetAllBackup().Where(x => File.Exists(x.BackupPath)));

    [RelayCommand]
    private async Task Restore(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new BackupMessage(), "BackupRestore"))
        {
            backupService.Restore(backupItem);
        }
    }

    [RelayCommand]
    private async Task Delete(IBackupItem backupItem)
    {
        if (await WeakReferenceMessenger.Default.Send(new BackupMessage(), "BackupDelete"))
        {
            BackupItems.Remove(backupItem);
            backupService.Delete(backupItem);
        }
    }
}