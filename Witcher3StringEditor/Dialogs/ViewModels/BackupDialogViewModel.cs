using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class BackupDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly BackupManger backupManger = BackupManger.Instance;

    public ObservableCollection<IBackupItem> BackupItems => backupManger.BackupItems;

    public event EventHandler? RequestClose;

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Restore(BackupItem backupItem)
    {
        var message = new BackupActionMessage(BackupActionType.Restore);
        if (await WeakReferenceMessenger.Default.Send(message))
        {
            backupManger.Restore(backupItem);
        }
    }

    [RelayCommand]
    private async Task Delete(BackupItem backupItem)
    {
        var message = new BackupActionMessage(BackupActionType.Delete);
        if (await WeakReferenceMessenger.Default.Send(message))
        {
            backupManger.Delete(backupItem);
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}