namespace Witcher3StringEditor.Common.TranslationMemory;

/// <summary>
///     Provides translation memory settings for future integration wiring.
/// </summary>
public interface ITranslationMemorySettingsProvider
{
    /// <summary>
    ///     Builds the current translation memory settings snapshot.
    /// </summary>
    TranslationMemorySettings GetSettings();
}
