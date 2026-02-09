namespace Witcher3StringEditor.Integrations.Profiles;

public sealed record TranslationProfile(
    string Id,
    string Name,
    string ProviderName,
    string? ModelName,
    string? BaseUrl,
    string? TerminologyPath,
    string? TerminologyFilePath,
    string? StyleGuidePath,
    string? StyleGuideFilePath,
    bool? UseTerminologyPack,
    bool? UseStyleGuide,
    bool? UseTranslationMemory,
    string? Notes = null);
