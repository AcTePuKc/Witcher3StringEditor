namespace Witcher3StringEditor.Common.Translation;

public sealed record TranslationContext(
    string SourceLanguage,
    string TargetLanguage,
    string? ProviderName,
    string? ModelName,
    string? ProfileId,
    bool UseTerminologyPack,
    bool UseStyleGuide);
