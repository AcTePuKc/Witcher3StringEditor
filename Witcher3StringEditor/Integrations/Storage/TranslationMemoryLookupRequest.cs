namespace Witcher3StringEditor.Integrations.Storage;

public sealed record TranslationMemoryLookupRequest(
    string SourceText,
    string SourceLanguage,
    string TargetLanguage,
    string? ProviderName = null,
    string? ModelId = null,
    int? MaxResults = null,
    bool IncludeFuzzyMatches = false);
