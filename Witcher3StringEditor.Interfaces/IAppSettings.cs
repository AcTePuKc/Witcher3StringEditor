using System.Collections.ObjectModel;
using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Interfaces;

public interface IAppSettings
{
    public W3FileType PreferredW3FileType { get; set; }

    public W3Language PreferredLanguage { get; set; }

    public string W3StringsPath { get; set; }

    public string GameExePath { get; set; }

    public string NexusModUrl { get; }
    
    public IModelSettings ModelSettings { get; }

    public ObservableCollection<IRecentItem> RecentItems { get; }

    public ObservableCollection<IBackupItem> BackupItems { get; }
}