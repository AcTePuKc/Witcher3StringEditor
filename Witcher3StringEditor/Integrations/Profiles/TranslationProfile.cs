namespace Witcher3StringEditor.Integrations.Profiles;

public sealed record TranslationProfile(
    string Id,
    string DisplayName,
    string ProviderName,
    string? ModelId,
    string? TerminologyPath,
    string? TerminologyFilePath,
    string? StyleGuidePath,
    string? StyleGuideFilePath,
    bool? UseTerminologyPack,
    bool? UseStyleGuide,
    string? Notes = null);
