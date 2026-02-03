using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Models;

/// <summary>
///     Represents the application settings model
///     Implements the IAppSettings interface and provides observable properties for data binding
///     This class is used to store and manage application-level settings and preferences
/// </summary>
internal partial class AppSettings : ObservableObject, IAppSettings
{
    /// <summary>
    ///     Gets or sets the path to the game executable
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string gameExePath = string.Empty;

    /// <summary>
    ///     Gets or sets the application language
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string language = string.Empty;

    /// <summary>
    ///     Gets or sets the page size for pagination
    /// </summary>
    [ObservableProperty] private int pageSize = 30;

    /// <summary>
    ///     Gets or sets the preferred language for The Witcher 3 string operations
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private W3Language preferredLanguage;

    /// <summary>
    ///     Gets or sets the preferred The Witcher 3 file type for operations
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private W3FileType preferredW3FileType;

    /// <summary>
    ///     Gets or sets the preferred translator service
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string translator = "MicrosoftTranslator";

    /// <summary>
    ///     Gets or sets the translation provider name
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string translationProviderName = string.Empty;

    /// <summary>
    ///     Gets or sets the translation model name
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string translationModelName = string.Empty;

    /// <summary>
    ///     Gets or sets the translation provider base URL
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string translationBaseUrl = string.Empty;

    /// <summary>
    ///     Gets or sets the translation provider base URL (alias for TranslationBaseUrl)
    /// </summary>
    public string BaseUrl
    {
        get => TranslationBaseUrl;
        set => TranslationBaseUrl = value;
    }

    /// <summary>
    ///     Gets or sets the optional translation profile id
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string? translationProfileId;

    /// <summary>
    ///     Gets or sets the Syncfusion license key (optional)
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string? syncfusionLicenseKey;

    /// <summary>
    ///     Gets or sets the path to the W3Strings tool executable
    ///     This property supports data binding through the ObservableObject base class
    /// </summary>
    [ObservableProperty] private string w3StringsPath = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the AppSettings class
    ///     Creates an empty settings object with default values
    /// </summary>
    public AppSettings()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the AppSettings class with specified values
    ///     This constructor is used during JSON deserialization
    /// </summary>
    /// <param name="w3StringsPath">The path to the W3Strings tool executable</param>
    /// <param name="gameExePath">The path to the game executable</param>
    /// <param name="preferredW3FileType">The preferred The Witcher 3 file type</param>
    /// <param name="preferredLanguage">The preferred language</param>
    /// <param name="backupItems">The collection of backup items</param>
    /// <param name="recentItems">The collection of recent items</param>
    [JsonConstructor]
    public AppSettings(string w3StringsPath, string gameExePath, W3FileType preferredW3FileType,
        W3Language preferredLanguage, ObservableCollection<BackupItem> backupItems,
        ObservableCollection<RecentItem> recentItems)
    {
        GameExePath = gameExePath;
        W3StringsPath = w3StringsPath;
        BackupItems = [.. backupItems];
        RecentItems = [.. recentItems];
        PreferredW3FileType = preferredW3FileType;
        PreferredLanguage = preferredLanguage;
    }

    /// <summary>
    ///     Gets the URL to the NexusMods page for this application
    ///     This property is ignored during JSON serialization
    /// </summary>
    [JsonIgnore]
    [UsedImplicitly]
    public string NexusModUrl => "https://www.nexusmods.com/witcher3/mods/10032";

    /// <summary>
    ///     Gets the collection of recently opened items
    ///     This collection supports data binding through the ObservableObject base class
    /// </summary>
    public ObservableCollection<IRecentItem> RecentItems { get; } = [];

    /// <summary>
    ///     Gets the collection of backup items
    ///     This collection supports data binding through the ObservableObject base class
    /// </summary>
    public ObservableCollection<IBackupItem> BackupItems { get; } = [];
}
