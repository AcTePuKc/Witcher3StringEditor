using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using System.IO;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BackupDialogViewModel : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public ObservableCollection<IBackupItem> BackupItems { get; }

    public BackupDialogViewModel()
    {
        BackupItems = new ObservableCollection<IBackupItem>(BackupManger.Instance.GetAllBackup().Where(x => File.Exists(x.BackupPath)));
    }

    [RelayCommand]
    private static async Task Restore(BackupItem backupItem)
    {
        var message = new ReturnBooleanNothingMessage();
        if (await WeakReferenceMessenger.Default.Send(message, "BackupRestore"))
        {
            BackupManger.Instance.Restore(backupItem);
        }
    }

    [RelayCommand]
    private async Task Delete(BackupItem backupItem)
    {
        var message = new ReturnBooleanNothingMessage();
        if (await WeakReferenceMessenger.Default.Send(message, "BackupDelete"))
        {
            BackupItems.Remove(backupItem);
            BackupManger.Instance.Delete(backupItem);
        }
    }
}