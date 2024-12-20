using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

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
        if (await MessageBox.ShowAsync(Strings.BackupRestoreMessage, Strings.Warning, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes) backupManger.Restore(backupItem);
    }

    [RelayCommand]
    private async Task Delete(BackupItem backupItem)
    {
        if (await MessageBox.ShowAsync(Strings.BackupDeleteMessage, Strings.Warning, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes) backupManger.Delete(backupItem);
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}