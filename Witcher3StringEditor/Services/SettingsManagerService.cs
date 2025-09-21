using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Constants;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides settings management functionality
///     Implements the ISettingsManagerService interface to handle checking and applying application settings
/// </summary>
internal class SettingsManagerService : ISettingsManagerService
{
    /// <summary>
    ///     The application settings instance
    /// </summary>
    private readonly IAppSettings appSettings;

    /// <summary>
    ///     Initializes a new instance of the SettingsManagerService class
    /// </summary>
    /// <param name="appSettings">The application settings instance</param>
    public SettingsManagerService(IAppSettings appSettings)
    {
        this.appSettings = appSettings;

        if (this.appSettings is INotifyPropertyChanged notifyPropertyChanged)
            notifyPropertyChanged.PropertyChanged += OnAppSettingsPropertyChanged;
    }

    /// <summary>
    ///     Checks the current application settings and logs information about them
    ///     If required settings are missing, sends a message to trigger the first run setup
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task CheckSettings()
    {
        Log.Information("Checking whether the settings are correct.");
        if (string.IsNullOrWhiteSpace(appSettings.W3StringsPath))
        {
            Log.Error("Settings are incorrect or initial setup is incomplete.");
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), MessageTokens.FirstRun);
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

    /// <summary>
    ///     Handles the PropertyChanged event of the AppSettings object
    ///     Sends messages based on which property changed
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">A PropertyChangedEventArgs that contains the event data</param>
    private void OnAppSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Handle different property changes with appropriate actions
        switch (e.PropertyName)
        {
            case nameof(IAppSettings.W3StringsPath):
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true),
                    MessageTokens.W3StringsPathChanged);
                break;
            case nameof(IAppSettings.GameExePath):
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true),
                    MessageTokens.GameExePathChanged);
                break;
            case nameof(IAppSettings.Translator):
                ApplyTranslatorChange(appSettings);
                break;
            case nameof(IAppSettings.Language):
                ApplyLanguageChange(appSettings.Language);
                break;
        }
    }

    /// <summary>
    ///     Applies the translator change and logs the new translator
    /// </summary>
    /// <param name="appSettings">The application settings instance</param>
    private static void ApplyTranslatorChange(IAppSettings appSettings)
    {
        Log.Information("Translator changed to {Translator}", appSettings.Translator);
    }

    /// <summary>
    ///     Applies the language change and updates the culture
    /// </summary>
    /// <param name="language">The new language code</param>
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