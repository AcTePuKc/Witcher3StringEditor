namespace Witcher3StringEditor.Common.Constants;

/// <summary>
///     Provides a centralized collection of message tokens used for communication between components
///     These tokens are used with the messaging system to identify specific types of messages
/// </summary>
public static class MessageTokens
{
    /// <summary>
    ///     Token for messages indicating that the translator is currently busy
    /// </summary>
    public const string TranslatorIsBusy = "TranslatorIsBusy";

    /// <summary>
    ///     Token for messages indicating that a translation is not empty
    /// </summary>
    public const string TranslationNotEmpty = "TranslationNotEmpty";

    /// <summary>
    ///     Token for messages indicating that translated text is invalid
    /// </summary>
    public const string TranslatedTextInvalid = "TranslatedTextInvalid";

    /// <summary>
    ///     Token for messages indicating that a translation error has occurred
    /// </summary>
    public const string TranslateError = "TranslateError";

    /// <summary>
    ///     Token for messages indicating that translated text has not been saved
    /// </summary>
    public const string TranslatedTextNoSaved = "TranslatedTextNoSaved";

    /// <summary>
    ///     Token for messages indicating that search results have been updated
    /// </summary>
    public const string SearchResultsUpdated = "SearchResultsUpdated";

    /// <summary>
    ///     Token for messages indicating that items have been added
    /// </summary>
    public const string ItemsAdded = "ItemsAdded";

    /// <summary>
    ///     Token for messages indicating that items have been removed
    /// </summary>
    public const string ItemsRemoved = "ItemsRemoved";

    /// <summary>
    ///     Token for messages indicating that search should be cleared
    /// </summary>
    public const string ClearSearch = "ClearSearch";

    /// <summary>
    ///     Token for messages indicating that the data grid should be refreshed
    /// </summary>
    public const string RefreshDataGrid = "RefreshDataGrid";

    /// <summary>
    ///     Token for messages indicating that a recent file has been opened
    /// </summary>
    public const string RecentFileOpened = "RecentFileOpened";

    /// <summary>
    ///     Token for messages indicating that a file should be reopened
    /// </summary>
    public const string ReOpenFile = "ReOpenFile";

    /// <summary>
    ///     Token for messages indicating that this is the first run of the application
    /// </summary>
    public const string FirstRun = "FirstRun";

    /// <summary>
    ///     Token for messages indicating that the main window is closing
    /// </summary>
    public const string MainWindowClosing = "MainWindowClosing";

    /// <summary>
    ///     Token for messages related to backup restoration
    /// </summary>
    public const string BackupRestore = "BackupRestore";

    /// <summary>
    ///     Token for messages indicating that a backup file was not found
    /// </summary>
    public const string BackupFileNoFound = "BackupFileNoFound";

    /// <summary>
    ///     Token for messages related to backup deletion
    /// </summary>
    public const string BackupDelete = "BackupDelete";

    /// <summary>
    ///     Token for messages indicating that an operation has failed
    /// </summary>
    public const string OperationFailed = "OperationFailed";

    /// <summary>
    ///     Token for messages indicating that the W3Strings path has changed
    /// </summary>
    public const string W3StringsPathChanged = "W3StringsPathChanged";

    /// <summary>
    ///     Token for messages indicating that the game executable path has changed
    /// </summary>
    public const string GameExePathChanged = "GameExePathChanged";

    /// <summary>
    ///     Token for messages related to save operations
    /// </summary>
    public const string Save = "Save";

    /// <summary>
    ///     Token for messages indicating that translation mode should be switched
    /// </summary>
    public const string TranslationModeSwitch = "TranslationModeSwitch";

    /// <summary>
    ///     Token for messages indicating that the translation dialog is closing
    /// </summary>
    public const string TranslationDialogClosing = "TranslationDialogClosing";

    /// <summary>
    ///     Token for messages indicating that an opened file was not found
    /// </summary>
    public const string OpenedFileNoFound = "OpenedFileNoFound";

    /// <summary>
    ///     Token for messages related to recent items
    /// </summary>
    public const string RecentItem = "RecentItem";
}