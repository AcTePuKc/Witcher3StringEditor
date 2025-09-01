using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Shared.Abstractions;

public interface IAppSettings
{
    public W3FileType PreferredW3FileType { get; set; }

    public W3Language PreferredLanguage { get; set; }

    public string W3StringsPath { get; set; }

    public string GameExePath { get; set; }

    public string NexusModUrl { get; }

    public string Translator { get; set; }

    public ObservableCollection<IRecentItem> RecentItems { get; }

    public ObservableCollection<IBackupItem> BackupItems { get; }

    public string Language { get; set; }
}