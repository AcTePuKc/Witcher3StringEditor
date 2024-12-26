using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class RecentDialogViewModel(IAppSettings appSettings) : ObservableObject, IModalDialogViewModel, ICloseable
{
    public bool? DialogResult => true;

    public event EventHandler? RequestClose;

    public IAppSettings AppSettings => appSettings;

    [RelayCommand]
    private void Open(IRecentItem item)
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "RecentFileOpened");
    }
}