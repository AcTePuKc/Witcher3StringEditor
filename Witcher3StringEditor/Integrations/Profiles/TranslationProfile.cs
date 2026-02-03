namespace Witcher3StringEditor.Integrations.Profiles;

public sealed record TranslationProfile(
    string Id,
    string DisplayName,
    string ProviderName,
    string? ModelId,
    string? TerminologyPath,
    string? StyleGuidePath,
    string? Notes = null);
