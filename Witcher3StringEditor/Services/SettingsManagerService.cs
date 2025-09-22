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
        Log.Information("Checking whether the settings are correct."); // Log start of settings check
        if (string.IsNullOrWhiteSpace(appSettings.W3StringsPath)) // Check if W3Strings path is set
        {
            Log.Error("Settings are incorrect or initial setup is incomplete."); // Log error if not set
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                MessageTokens.FirstRun); // Trigger first run
        }
        else // If W3Strings path is set
        {
            Log.Information("The W3Strings path has been set to {Path}.",
                appSettings.W3StringsPath); // Log W3Strings path
            if (string.IsNullOrWhiteSpace(appSettings.GameExePath)) // Check if game exe path is set
                Log.Warning("The game executable path is not set."); // Log warning if not set
            else // If game exe path is set
                Log.Information("The game executable path has been set to {Path}.",
                    appSettings.GameExePath); // Log game exe path
            Log.Information("The preferred filetype is {Filetype}",
                appSettings.PreferredW3FileType); // Log preferred file type
            Log.Information("The preferred language is {Language}",
                appSettings.PreferredLanguage); // Log preferred language
            Log.Information("Current translator is {Translator}.", appSettings.Translator); // Log current translator
            Log.Information("Settings are correct."); // Log that settings are correct
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
        switch (e.PropertyName) // Switch on property name
        {
            case nameof(IAppSettings.W3StringsPath): // If W3StringsPath changed
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), // Send message
                    MessageTokens.W3StringsPathChanged);
                break;
            case nameof(IAppSettings.GameExePath): // If GameExePath changed
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), // Send message
                    MessageTokens.GameExePathChanged);
                break;
            case nameof(IAppSettings.Translator): // If Translator changed
                ApplyTranslatorChange(appSettings); // Apply translator change
                break;
            case nameof(IAppSettings.Language): // If Language changed
                ApplyLanguageChange(appSettings.Language); // Apply language change
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