using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class SettingDialogViewModel(Settings settings) : ObservableObject, IModalDialogViewModel
{
    private readonly IDialogService dialogService = Ioc.Default.GetRequiredService<IDialogService>();

    [ObservableProperty]
    private Settings settings = settings;

    public bool? DialogResult => true;

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
        if (storageFile is { Name: "w3strings.exe" }) Settings.W3StringsPath = storageFile.LocalPath;
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
        if (storageFile is { Name: "witcher3.exe" }) Settings.GameExePath = storageFile.LocalPath;
    }
}