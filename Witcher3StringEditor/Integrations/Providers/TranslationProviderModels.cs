namespace Witcher3StringEditor.Integrations.Providers;

public sealed record TranslationRequest(
    string SourceText,
    string SourceLanguage,
    string TargetLanguage,
    string? GlossaryPath = null,
    string? StyleGuidePath = null,
    string? ProfileId = null,
    string? ModelId = null);

public sealed record TranslationResult(
    string? TranslatedText,
    string ProviderName,
    string? ModelId,
    string? Notes = null);

public sealed record TranslationProviderModel(
    string Id,
    string? DisplayName,
    bool IsDefault = false);
