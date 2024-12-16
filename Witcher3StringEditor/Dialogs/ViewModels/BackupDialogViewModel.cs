using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class BackupDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    public event EventHandler? RequestClose;
    public bool? DialogResult => true;

    [RelayCommand]
    private static async Task Restore(BackupItem backupItem)
    {
        if (await MessageBox.ShowAsync(Strings.BackupRestoreMessage, Strings.Warning, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes) BackupManger.Restore(backupItem);
    }

    [RelayCommand]
    private static async Task DeleteAsync(BackupItem backupItem)
    {
        if (await MessageBox.ShowAsync(Strings.BackupDeleteMessage, Strings.Warning, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes) BackupManger.Delete(backupItem);
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}