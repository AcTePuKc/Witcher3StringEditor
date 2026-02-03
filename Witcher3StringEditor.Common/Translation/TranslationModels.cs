using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Translation;

public sealed class TranslationRequest
{
    public string? SourceLanguage { get; init; }

    public string? TargetLanguage { get; init; }

    public string Text { get; init; } = string.Empty;

    public string? ModelId { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

public sealed class TranslationResult
{
    public string TranslatedText { get; init; } = string.Empty;

    public string? ModelId { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

public sealed class ModelInfo
{
    public string Id { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
