using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Translation;

public sealed record TranslationPostProcessingRequest(
    string Text,
    TranslationPipelineContext Context,
    string SourceLanguage,
    string TargetLanguage);

public sealed record TranslationPostProcessingResult(
    string Text,
    IReadOnlyList<string> AppliedRules,
    IReadOnlyDictionary<string, string> Metadata);
