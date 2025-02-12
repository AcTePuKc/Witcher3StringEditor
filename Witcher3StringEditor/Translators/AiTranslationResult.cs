using GTranslate;
using GTranslate.Results;

namespace Witcher3StringEditor.Translators;

internal class AiTranslationResult : ITranslationResult
{
    public required string Translation { get; init; }

    public required string Source { get; init; }

    public string Service => "AiTranslator";

    public required ILanguage SourceLanguage { get; init; }

    public required ILanguage TargetLanguage { get; init; }
}