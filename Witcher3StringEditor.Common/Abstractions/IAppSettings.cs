using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Common.Abstractions;

/// <summary>
///     Defines a contract for application settings management
///     Provides access to user preferences, paths, and collections of recent and backup items
/// </summary>
public interface IAppSettings
{
    /// <summary>
    ///     Gets or sets the preferred The Witcher 3 file type for operations
    /// </summary>
    public W3FileType PreferredW3FileType { get; set; }

    /// <summary>
    ///     Gets or sets the preferred language for the application
    /// </summary>
    public W3Language PreferredLanguage { get; set; }

    /// <summary>
    ///     Gets or sets the path to the W3Strings tool executable
    /// </summary>
    public string W3StringsPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the game executable
    /// </summary>
    public string GameExePath { get; set; }

    /// <summary>
    ///     Gets the URL to the NexusMods page for this application
    /// </summary>
    public string NexusModUrl { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the app should auto-check for updates on startup
    /// </summary>
    public bool AutoCheckUpdates { get; set; }

    /// <summary>
    ///     Gets or sets the preferred translator service
    /// </summary>
    public string Translator { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether provider failures should fall back to legacy translators
    /// </summary>
    public bool UseLegacyTranslationFallback { get; set; }

    /// <summary>
    ///     Gets or sets the translation provider name (future integration)
    /// </summary>
    public string TranslationProviderName { get; set; }

    /// <summary>
    ///     Gets or sets the translation model name (future integration)
    /// </summary>
    public string TranslationModelName { get; set; }

    /// <summary>
    ///     Gets or sets the cached translation model list (future integration)
    /// </summary>
    public ObservableCollection<string> CachedTranslationModels { get; set; }

    /// <summary>
    ///     Gets or sets the translation provider timeout in seconds (future integration)
    /// </summary>
    public int TranslationProviderTimeoutSeconds { get; set; }

    /// <summary>
    ///     Gets or sets the translation provider base URL (alias for TranslationBaseUrl)
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    ///     Gets or sets the translation provider base URL (future integration)
    /// </summary>
    public string TranslationBaseUrl { get; set; }

    /// <summary>
    ///     Gets or sets the terminology file path (future integration)
    /// </summary>
    public string TerminologyFilePath { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the terminology pack is enabled (future integration)
    /// </summary>
    public bool UseTerminologyPack { get; set; }

    /// <summary>
    ///     Gets or sets the style guide file path (future integration)
    /// </summary>
    public string StyleGuideFilePath { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the style guide is enabled (future integration)
    /// </summary>
    public bool UseStyleGuide { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether translation memory is enabled (future integration)
    /// </summary>
    public bool UseTranslationMemory { get; set; }

    /// <summary>
    ///     Gets or sets the translation memory database path (future integration)
    /// </summary>
    public string TranslationMemoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the optional translation profile id (future integration)
    /// </summary>
    public string? TranslationProfileId { get; set; }

    /// <summary>
    ///     Gets or sets the Syncfusion license key (optional)
    /// </summary>
    public string? SyncfusionLicenseKey { get; set; }

    /// <summary>
    ///     Gets the collection of recently opened items
    /// </summary>
    public ObservableCollection<IRecentItem> RecentItems { get; }

    /// <summary>
    ///     Gets the collection of backup items
    /// </summary>
    public ObservableCollection<IBackupItem> BackupItems { get; }

    /// <summary>
    ///     Gets or sets the application language
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    ///     Gets or sets the page size for pagination
    /// </summary>
    public int PageSize { get; set; }
}
