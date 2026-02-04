namespace Witcher3StringEditor.Common.TranslationMemory;

/// <summary>
///     Represents settings for the translation memory feature.
/// </summary>
public sealed class TranslationMemorySettings
{
    /// <summary>
    ///     Gets a value indicating whether translation memory is enabled.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    ///     Gets the path to the local translation memory database.
    /// </summary>
    public string? DatabasePath { get; init; }

    /// <summary>
    ///     Gets the name of the translation memory provider.
    /// </summary>
    public string? ProviderName { get; init; }
}
