using System;

namespace Witcher3StringEditor.Common.TranslationMemory;

public sealed class TranslationMemoryEntry
{
    public string SourceText { get; init; } = string.Empty;

    public string TargetText { get; init; } = string.Empty;

    public string? SourceLanguage { get; init; }

    public string? TargetLanguage { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class TranslationMemoryQuery
{
    public string SourceText { get; init; } = string.Empty;

    public string? SourceLanguage { get; init; }

    public string? TargetLanguage { get; init; }
}
