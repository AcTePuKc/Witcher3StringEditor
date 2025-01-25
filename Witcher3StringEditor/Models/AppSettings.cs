using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class AppSettings : ObservableObject, IAppSettings
{
    [ObservableProperty]
    private FileType preferredFileType;

    [ObservableProperty]
    private W3Language preferredLanguage;

    [ObservableProperty]
    private string w3StringsPath = string.Empty;

    [ObservableProperty]
    private string gameExePath = string.Empty;

    public ObservableCollection<IRecentItem> RecentItems { get; set; } = [];

    public ObservableCollection<IBackupItem> BackupItems { get; set; } = [];

    public string NexusModUrl { get; set; } = "https://www.nexusmods.com/witcher3/mods/10032";

    public AppSettings()
    {
    }

    [JsonConstructor]
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
        BackupItems = new ObservableCollection<IBackupItem>(backupItems);
        RecentItems = new ObservableCollection<IRecentItem>(recentItems);
    }
}