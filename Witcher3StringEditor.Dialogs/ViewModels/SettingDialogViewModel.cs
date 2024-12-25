using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SettingDialogViewModel(IAppSettings appSettings, IDialogService dialogService) : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public IAppSettings AppSettings { get; } = appSettings;

    [RelayCommand]
    private async Task SetW3StringsPath()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters = [new FileFilter("w3strings.exe", ".exe")],
            Title = Strings.SelectW3Strings,
            SuggestedFileName = "w3strings"
        };
        var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "w3strings.exe" })
        {
            AppSettings.W3StringsExePath = storageFile.LocalPath;
        }
    }

    [RelayCommand]
    private async Task SetGameExePath()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters = [new FileFilter("witcher3.exe", ".exe")],
            Title = Strings.SelectGameExe,
            SuggestedFileName = "witcher3"
        };
        var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "witcher3.exe" })
        {
            AppSettings.GameExePath = storageFile.LocalPath;
        }
    }
}