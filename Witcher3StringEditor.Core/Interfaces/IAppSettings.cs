using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IAppSettings
    {
        public IEnumerable<IRecentItem>? RecentItems { get; set; }

        public IEnumerable<IBackupItem>? BackupItems { get; set; }

        public FileType PreferredFileType { get; set; }

        public W3Language PreferredLanguage { get; set; }

        public string W3StringsExePath { get; set; }

        public string GameExePath { get; set; }
    }
}