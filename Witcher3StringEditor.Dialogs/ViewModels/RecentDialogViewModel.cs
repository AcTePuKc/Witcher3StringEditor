using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.IO;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class RecentDialogViewModel(IAppSettings appSettings) : ObservableObject, IModalDialogViewModel, ICloseable
{
    public bool? DialogResult => true;

    public event EventHandler? RequestClose;

    public IAppSettings AppSettings => appSettings;

    [RelayCommand]
    private async Task Open(IRecentItem item)
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        if (File.Exists(item.FilePath))
        {
            await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "RecentFileOpened");
        }
        else
        {
            if (await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "OpenedFileNoFound"))
                appSettings.RecentItems.Remove(item);
        }
    }
}