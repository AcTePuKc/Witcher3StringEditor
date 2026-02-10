namespace Witcher3StringEditor.Common.Settings;

/// <summary>
///     Defines explicit user-triggered load operations for settings surfaces.
///     Implementations should avoid constructor-time file/network/profile/model work.
/// </summary>
public interface ISettingsDeferredLoadService
{
    /// <summary>
    ///     Loads cached local model options (no network call).
    /// </summary>
    Task<IReadOnlyList<string>> LoadCachedModelsAsync();

    /// <summary>
    ///     Loads profile summaries on explicit user action.
    /// </summary>
    Task LoadProfilesAsync();

    /// <summary>
    ///     Loads terminology/status metadata on explicit user action.
    /// </summary>
    Task LoadTerminologyAsync();

    /// <summary>
    ///     Loads style-guide/status metadata on explicit user action.
    /// </summary>
    Task LoadStyleGuideAsync();
}
