using System.Collections.ObjectModel;
using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Interfaces;

public interface IAppSettings
{
    public FileType PreferredFileType { get; set; }

    public W3Language PreferredLanguage { get; set; }

    public string W3StringsPath { get; set; }

    public string GameExePath { get; set; }

    public ObservableCollection<IRecentItem> RecentItems { get; set; }

    public ObservableCollection<IBackupItem> BackupItems { get; set; }
}