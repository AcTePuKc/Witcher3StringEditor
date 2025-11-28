using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Messaging;

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
        this.appSettings = appSettings; // Store the app settings instance
        if (this.appSettings is INotifyPropertyChanged
            notifyPropertyChanged) // Check if app settings supports property change notifications
            notifyPropertyChanged.PropertyChanged += OnAppSettingsPropertyChanged; // Register property change handler
    }

    /// <summary>
    ///     Checks the current application settings and logs information about them
    ///     If required settings are missing, sends a message to trigger the first run setup
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task CheckSettings()
    {
        Log.Information("Checking whether the settings are correct."); // Log start of settings check
    
        var hasErrors = false; // Flag to indicate whether there are any errors

        if (string.IsNullOrWhiteSpace(appSettings.W3StringsPath)) // If W3StringsPath is unset
        {
            Log.Error("Settings are incorrect or initial setup is incomplete."); // Log error with W3Strings path is unset
            _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), MessageTokens.FirstRun); // Send message to trigger first run setup
            return; // Return
        }

        if (!File.Exists(appSettings.W3StringsPath)) // If W3StringsPath does not exist
        {
            Log.Error("The W3Strings path is invalid: {Path}", appSettings.W3StringsPath); // Log error with invalid W3Strings path
            hasErrors = true; // Set flag to indicate error
        }
        else // If W3StringsPath exists
        {
            Log.Information("The W3Strings path has been set to {Path}.", appSettings.W3StringsPath); // Log W3Strings path
        }
        
        if (!string.IsNullOrWhiteSpace(appSettings.GameExePath)) // Log game executable path
        {
            if (!File.Exists(appSettings.GameExePath))  // If game executable path does not exist
            {
                Log.Error("The game executable path is invalid: {Path}", appSettings.GameExePath); // Log error if game executable path is invalid
                hasErrors = true; // Set flag to indicate error
            }
            else // If game executable path exists
            {
                Log.Information("The game executable path has been set to {Path}.", appSettings.GameExePath); // Log game executable path
            }
        }
        else // If game executable path is unset
        {
            Log.Warning("The game executable path is unset."); // Log warning if game executable path is unset
        }
        
        // Log preferred filetype, preferred language, and translator
        Log.Information("The preferred filetype is {Filetype}", appSettings.PreferredW3FileType); // Log preferred filetype
        Log.Information("The preferred language is {Language}", appSettings.PreferredLanguage); // Log preferred language
        Log.Information("Current translator is {Translator}.", appSettings.Translator); // Log translator
        
        if (hasErrors) // If there are errors
        {
            _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), MessageTokens.PathInvalid); // Send message to trigger path invalid
        }
        else // If no errors
        {
            Log.Information("Settings are correct."); // Log correct settings
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
                _ = WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), // Send message
                    MessageTokens.W3StringsPathChanged);
                break;
            case nameof(IAppSettings.GameExePath): // If GameExePath changed
                _ = WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), // Send message
                    MessageTokens.GameExePathChanged);
                break;
            case nameof(IAppSettings.Translator): // If Translator changed
                ApplyTranslatorChange(appSettings); // Apply translator change
                break;
            case nameof(IAppSettings.Language): // If Language changed
                ApplyLanguageChange(appSettings.Language); // Apply language change
                break;
            case nameof(IAppSettings.PageSize): // If PageSize changed
                _ = WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<int>(((IAppSettings)sender!).PageSize), // Send message
                    MessageTokens.PageSizeChanged);
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
            I18NExtension.Culture = new CultureInfo(language); // Set the new culture
            Log.Information("Language changed to {Language}.", language); // Log successful language change
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to change language."); // Log any errors during language change
        }
    }
}