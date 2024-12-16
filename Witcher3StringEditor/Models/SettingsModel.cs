using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Models;

public partial class SettingsModel : ObservableObject
{
    [ObservableProperty] private string gameExePath = string.Empty;

    [ObservableProperty] private FileType preferredFileType;

    [ObservableProperty] private W3Language preferredLanguage;

    [ObservableProperty] private string w3StringsPath = string.Empty;
}