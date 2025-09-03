using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SettingDialogViewModel(
    IAppSettings appSettings,
    IDialogService dialogService,
    IEnumerable<ITranslator> translators,
    ICultureResolver cultureResolver,
    ILogger<SettingDialogViewModel> logger)
    : ObservableObject, IModalDialogViewModel
{
    public IAppSettings AppSettings { get; } = appSettings;

    public IEnumerable<ITranslator> Translators { get; } = translators;

    public IEnumerable<CultureInfo> SupportedCultures { get; } = cultureResolver.SupportedCultures;

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
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "w3strings.exe" })
        {
            AppSettings.W3StringsPath = storageFile.LocalPath;
            logger.LogInformation("Encoder Path set to {Path}.", storageFile.LocalPath);
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
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is { Name: "witcher3.exe" })
        {
            AppSettings.GameExePath = storageFile.LocalPath;
            logger.LogInformation("Game Path set to {Path}.", storageFile.LocalPath);
        }
    }

    [RelayCommand]
    private void ChangeLanguage()
    {
        WeakReferenceMessenger.Default.Send(
            new ValueChangedMessage<CultureInfo>(new CultureInfo(AppSettings.Language)));
    }
}