using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class AppSettings : ObservableObject, IAppSettings
{
    [ObservableProperty]
    private FileType preferredFileType;

    [ObservableProperty]
    private W3Language preferredLanguage;

    [ObservableProperty]
    private string w3StringsPath;

    [ObservableProperty]
    private string gameExePath;

    public ObservableCollection<IRecentItem> RecentItems { get; set; }

    public ObservableCollection<IBackupItem> BackupItems { get; set; }

    public AppSettings(string w3StringsPath,
                       FileType preferredFileType,
                       W3Language preferredLanguage,
                       string gameExePath,
                       ObservableCollection<BackupItem> backupItems,
                       ObservableCollection<RecentItem> recentItems)
    {
        W3StringsPath = w3StringsPath;
        PreferredFileType = preferredFileType;
        PreferredLanguage = preferredLanguage;
        GameExePath = gameExePath;
        BackupItems = new(backupItems);
        RecentItems = new(recentItems);
    }
}