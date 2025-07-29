using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentValidation;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SettingDialogViewModel(
    IAppSettings appSettings,
    IDialogService dialogService,
    IValidator<IAppSettings> appSettingsValidator,
    IValidator<IModelSettings> modelSettingsValidator) : ObservableObject, IModalDialogViewModel
{
    public IAppSettings AppSettings { get; } = appSettings;
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
            Log.Information("Encoder Path set to {0}.", storageFile.LocalPath);
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
            Log.Information("Game Path set to {0}.", storageFile.LocalPath);
        }
    }

    [RelayCommand]
    private async Task ShowModelSettingDialog()
    {
        await dialogService.ShowDialogAsync<ModelSettingsDialogViewModel>(this,
            new ModelSettingsDialogViewModel(AppSettings.ModelSettings));
    }

    [RelayCommand]
    private async Task ShowPromotsSettingDialog()
    {
        await dialogService.ShowDialogAsync<PromptsSettingDialogViewModel>(this,
            new PromptsSettingDialogViewModel(AppSettings.ModelSettings));
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        var result = await appSettingsValidator.ValidateAsync(AppSettings);
        if (!result.IsValid)
        {
            e.Cancel = true;
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "InitializationIncomplete"))
                Application.Current.Shutdown();
        }
        else if (AppSettings.IsUseAiTranslate)
        {
            result = await modelSettingsValidator.ValidateAsync(AppSettings.ModelSettings);
            if (!result.IsValid)
            {
                if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                        "IncompleteAiTranslationSettings"))
                    AppSettings.IsUseAiTranslate = false;
                else
                    e.Cancel = true;
            }
        }
    }
}