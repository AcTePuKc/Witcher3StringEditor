using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class AppSettings : ObservableObject, IAppSettings
{
    [ObservableProperty] private string gameExePath = string.Empty;

    [ObservableProperty] private bool isUseAiTranslate;

    [ObservableProperty] private bool isUseKnowledgeBase;

    [ObservableProperty] private IModelSettings modelSettings = new ModelSettings();

    [ObservableProperty] private IEmbeddedModelSettings embeddedModelSettings = new EmbeddedModelSettings();

    [ObservableProperty] private W3Language preferredLanguage;

    [ObservableProperty] private W3FileType preferredW3FileType;

    [ObservableProperty] private string w3StringsPath = string.Empty;

    public AppSettings()
    {
    }

    [JsonConstructor]
    public AppSettings(string w3StringsPath,
        W3FileType preferredW3FileType,
        W3Language preferredLanguage,
        string gameExePath,
        ModelSettings? modelSettings,
        EmbeddedModelSettings? embeddedModelSettings,
        ObservableCollection<BackupItem> backupItems,
        ObservableCollection<RecentItem> recentItems)
    {
        W3StringsPath = w3StringsPath;
        PreferredW3FileType = preferredW3FileType;
        PreferredLanguage = preferredLanguage;
        GameExePath = gameExePath;
        BackupItems = [.. backupItems];
        RecentItems = [.. recentItems];
        ModelSettings = modelSettings ?? new ModelSettings();
        EmbeddedModelSettings = embeddedModelSettings ?? new EmbeddedModelSettings();
    }

    public ObservableCollection<IRecentItem> RecentItems { get; } = [];

    public ObservableCollection<IBackupItem> BackupItems { get; } = [];

    [JsonIgnore] public string NexusModUrl => "https://www.nexusmods.com/witcher3/mods/10032";
}