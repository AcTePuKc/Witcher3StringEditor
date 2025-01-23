using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using System.ComponentModel;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Dialogs.Validators;
using Witcher3StringEditor.Interfaces;

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
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "w3strings.exe" })
            AppSettings.W3StringsPath = storageFile.LocalPath;
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
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "witcher3.exe" })
            AppSettings.GameExePath = storageFile.LocalPath;
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        var validations = AppSettingsValidator.Instance;
        var result = await validations.ValidateAsync(AppSettings);
        if (!result.IsValid)
        {
            e.Cancel = true;
            if (await WeakReferenceMessenger.Default.Send(new WindowClosingMessage(), "SettingsDialogClosing"))
                Environment.Exit(0);
        }
    }
}