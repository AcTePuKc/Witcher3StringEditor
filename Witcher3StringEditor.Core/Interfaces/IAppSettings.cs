using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IAppSettings
    {
        public FileType PreferredFileType { get; set; }

        public W3Language PreferredLanguage { get; set; }

        public string W3StringsPath { get; set; }

        public string GameExePath { get; set; }
    }
}