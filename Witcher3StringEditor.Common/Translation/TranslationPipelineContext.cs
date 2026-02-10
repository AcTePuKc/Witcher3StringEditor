using System;
using System.Collections.Generic;

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
    ///     Gets the selected provider identifier (if any).
    /// </summary>
    public string? ProviderId { get; init; }

    /// <summary>
    ///     Gets the selected model identifier (if any).
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    ///     Gets the terminology pack paths (if any).
    /// </summary>
    public IReadOnlyList<string> TerminologyPaths { get; init; } = Array.Empty<string>();

    /// <summary>
    ///     Gets the style guide path (if any).
    /// </summary>
    public string? StyleGuidePath { get; init; }

    /// <summary>
    ///     Gets a value indicating whether translation memory should be used.
    /// </summary>
    public bool UseTranslationMemory { get; init; }
}
