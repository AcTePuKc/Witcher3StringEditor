namespace Witcher3StringEditor.Common.Translation;

/// <summary>
///     Provides future pipeline context for translation providers, terminology, profiles, and translation memory.
/// </summary>
public sealed class TranslationPipelineContext
{
    /// <summary>
    ///     Gets the selected translation profile identifier (if any).
    /// </summary>
    public string? ProfileId { get; init; }

    /// <summary>
    ///     Gets the selected provider name (if any).
    /// </summary>
    public string? ProviderName { get; init; }

    /// <summary>
    ///     Gets the selected model name (if any).
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    ///     Gets the terminology pack path (if any).
    /// </summary>
    public string? TerminologyPath { get; init; }

    /// <summary>
    ///     Gets the style guide path (if any).
    /// </summary>
    public string? StyleGuidePath { get; init; }

    /// <summary>
    ///     Gets a value indicating whether translation memory should be used.
    /// </summary>
    public bool UseTranslationMemory { get; init; }
}
