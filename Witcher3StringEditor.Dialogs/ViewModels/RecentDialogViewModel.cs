using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class RecentDialogViewModel(IAppSettings appSettings)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public IAppSettings AppSettings => appSettings;

    public event EventHandler? RequestClose;

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Open(IRecentItem item)
    {
        if (!File.Exists(item.FilePath))
        {
            Log.Error("The file {0} for the recent item being opened does not exist.", item.FilePath);
            if (await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "OpenedFileNoFound"))
            {
                _ = AppSettings.RecentItems.Remove(item);
                Log.Information("The recent item for file {0} has been deleted.", item.FilePath);
            }
        }
        else
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
            var isApproved =
                WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "RecentFileOpened");
            Log.Information("Recent item opening has been approved: {0}.", isApproved);
        }
    }
}