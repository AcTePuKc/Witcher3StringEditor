namespace Witcher3StringEditor.Common.Profiles;

public sealed class TranslationProfile
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string ProviderName { get; init; } = string.Empty;

    public string ModelName { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string? TerminologyPath { get; init; }

    public string? StyleGuidePath { get; init; }

    public bool? UseTranslationMemory { get; init; }

    public string? Notes { get; init; }
}
