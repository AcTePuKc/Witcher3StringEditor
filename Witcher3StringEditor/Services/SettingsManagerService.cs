using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

internal class SettingsManagerService : ISettingsManagerService
{
    private readonly IAppSettings appSettings;

    public SettingsManagerService(IAppSettings appSettings)
    {
        this.appSettings = appSettings;

        if (this.appSettings is INotifyPropertyChanged notifyPropertyChanged)
            notifyPropertyChanged.PropertyChanged += OnAppSettingsPropertyChanged;
    }

    public async Task CheckSettings()
    {
        Log.Information("Checking whether the settings are correct.");
        if (string.IsNullOrWhiteSpace(appSettings.W3StringsPath))
        {
            Log.Error("Settings are incorrect or initial setup is incomplete.");
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "FirstRun");
        }
        else
        {
            Log.Information("The W3Strings path has been set to {Path}.", appSettings.W3StringsPath);
            if (string.IsNullOrWhiteSpace(appSettings.GameExePath))
                Log.Warning("The game executable path is not set.");
            else
                Log.Information("The game executable path has been set to {Path}.", appSettings.GameExePath);
            Log.Information("The preferred filetype is {Filetype}", appSettings.PreferredW3FileType);
            Log.Information("The preferred language is {Language}", appSettings.PreferredLanguage);
            Log.Information("Current translator is {Translator}.", appSettings.Translator);
            Log.Information("Settings are correct.");
        }
    }

    private void OnAppSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IAppSettings.W3StringsPath):
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "W3StringsPathChanged");
                break;
            case nameof(IAppSettings.GameExePath):
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "GameExePathChanged");
                break;
            case nameof(IAppSettings.Translator):
                ApplyTranslatorChange(appSettings);
                break;
            case nameof(IAppSettings.Language):
                ApplyLanguageChange(appSettings.Language);
                break;
        }
    }

    private static void ApplyTranslatorChange(IAppSettings appSettings)
    {
        Log.Information("Translator changed to {Translator}", appSettings.Translator);
    }

    private static void ApplyLanguageChange(string language)
    {
        try
        {
            I18NExtension.Culture = new CultureInfo(language);
            Log.Information("Language changed to {Language}.", language);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to change language.");
        }
    }
}