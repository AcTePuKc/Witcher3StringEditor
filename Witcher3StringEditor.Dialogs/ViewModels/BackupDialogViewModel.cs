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

public partial class BackupDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    public bool? DialogResult => true;

    public event EventHandler? RequestClose;

    public ObservableCollection<IBackupItem> BackupItems { get; }

    public BackupDialogViewModel()
    {
        BackupItems = new(BackupManger.Instance.GetAllBackup().Where(x => File.Exists(x.BackupPath)));
    }

    [RelayCommand]
    private static async Task Restore(BackupItem backupItem)
    {
        var message = new BackupActionMessage(BackupActionType.restore);
        if (await WeakReferenceMessenger.Default.Send(message))
        {
            BackupManger.Instance.Restore(backupItem);
        }
    }

    [RelayCommand]
    private async Task Delete(BackupItem backupItem)
    {
        var message = new BackupActionMessage(BackupActionType.delete);
        if (await WeakReferenceMessenger.Default.Send(message))
        {
            BackupItems.Remove(backupItem);
            BackupManger.Instance.Delete(backupItem);
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}