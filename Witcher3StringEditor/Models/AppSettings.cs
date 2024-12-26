using CommunityToolkit.Mvvm.ComponentModel;
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
    private string w3StringsPath = string.Empty;

    [ObservableProperty]
    private string gameExePath = string.Empty;
}