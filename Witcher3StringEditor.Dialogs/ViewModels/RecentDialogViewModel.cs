using System.Collections.Specialized;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Messaging;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class RecentDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    public RecentDialogViewModel(IAppSettings appSettings)
    {
        AppSettings = appSettings;
        AppSettings.RecentItems.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
                Log.Information("Recent items collection changed: {Count} items removed.",
                    e.OldItems?.Count ?? 0);
        };
    }

    public IAppSettings AppSettings { get; }
    public event EventHandler? RequestClose;

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Open(IRecentItem item)
    {
        if (!File.Exists(item.FilePath))
            await HandleMissingFile(item);
        else
            HandleExistingFile(item);
    }

    private void HandleExistingFile(IRecentItem item)
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        _ = WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "RecentFileOpened");
    }

    private async Task HandleMissingFile(IRecentItem item)
    {
        LogMissingFile(item.FilePath);
        if (await NotifyFileNotFound(item.FilePath)) 
            TryRemoveRecentItem(item);
    }

    private void TryRemoveRecentItem(IRecentItem item)
    {
        if (AppSettings.RecentItems.Remove(item))
            Log.Information("The recent item for file {Path} has been removed.", item.FilePath);
        else
            Log.Error("The recent item for file {Path} could not be removed.", item.FilePath);
    }

    private static async Task<bool> NotifyFileNotFound(string filePath)
    {
        return await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(filePath), "OpenedFileNoFound");
    }

    private static void LogMissingFile(string filePath)
    {
        Log.Error("The file {Path} for the recent item being opened does not exist.", filePath);
    }
}