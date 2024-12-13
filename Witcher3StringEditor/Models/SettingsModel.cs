using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Models
{
    public partial class SettingsModel : ObservableObject
    {
        [ObservableProperty]
        private string w3StringsPath = string.Empty;

        [ObservableProperty]
        private string gameExePath = string.Empty;

        [ObservableProperty]
        private W3Language preferredLanguage;

        [ObservableProperty]
        private FileType preferredFileType;
    }
}