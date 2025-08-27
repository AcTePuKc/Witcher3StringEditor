using System.Collections.Specialized;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class RecentDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly ILogger<RecentDialogViewModel> logger;

    public RecentDialogViewModel(IAppSettings appSettings, ILogger<RecentDialogViewModel> logger)
    {
        this.logger = logger;
        AppSettings = appSettings;
        AppSettings.RecentItems.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
                logger.LogInformation("Recent items collection changed: {Count} items removed.",
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
        {
            logger.LogError("The file {Path} for the recent item being opened does not exist.", item.FilePath);
            if (await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "OpenedFileNoFound"))
            {
                _ = AppSettings.RecentItems.Remove(item);
                logger.LogInformation("The recent item for file {Path} has been removed.", item.FilePath);
            }
        }
        else
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
            var isApproved =
                WeakReferenceMessenger.Default.Send(new FileOpenedMessage(item.FilePath), "RecentFileOpened");
            logger.LogInformation("Recent item opening has been approved: {IsApproved}.", isApproved);
        }
    }
}