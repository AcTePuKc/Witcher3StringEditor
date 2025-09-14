using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
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
        WeakEventManager<ObservableCollection<IRecentItem>, NotifyCollectionChangedEventArgs>.AddHandler(
            AppSettings.RecentItems,
            nameof(IAppSettings.RecentItems.CollectionChanged),
            OnRecentItemsOnCollectionChanged);
    }

    public IAppSettings AppSettings { get; }
    public event EventHandler? RequestClose;

    public bool? DialogResult => true;

    private static void OnRecentItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
            Log.Information("Recent items collection changed: {Count} items removed.",
                e.OldItems?.Count ?? 0);
    }

    [RelayCommand]
    private async Task Open(IRecentItem recentItem)
    {
        if (!File.Exists(recentItem.FilePath))
            await HandleMissingFile(recentItem);
        else
            HandleExistingFile(recentItem);
    }

    private void HandleExistingFile(IRecentItem recentItem)
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        _ = WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<string, bool>(recentItem.FilePath),
            "RecentFileOpened");
    }

    private async Task HandleMissingFile(IRecentItem recentItem)
    {
        LogMissingFile(recentItem.FilePath);
        if (await NotifyFileNotFound(recentItem.FilePath))
            TryRemoveRecentItem(recentItem);
    }

    private void TryRemoveRecentItem(IRecentItem recentItem)
    {
        if (AppSettings.RecentItems.Remove(recentItem))
            Log.Information("The recent item for file {Path} has been removed.", recentItem.FilePath);
        else
            Log.Error("The recent item for file {Path} could not be removed.", recentItem.FilePath);
    }

    private static async Task<bool> NotifyFileNotFound(string filePath)
    {
        return await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<string, bool>(filePath),
            "OpenedFileNoFound");
    }

    private static void LogMissingFile(string filePath)
    {
        Log.Error("The file {Path} for the recent item being opened does not exist.", filePath);
    }
}