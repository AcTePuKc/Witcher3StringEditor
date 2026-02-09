namespace Witcher3StringEditor.Common.Translation;

public sealed record TranslationProviderFailureDto(
    string ProviderName,
    TranslationProviderFailureKind FailureKind,
    string Message);
