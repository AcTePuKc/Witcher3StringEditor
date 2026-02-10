using System.Collections.Generic;

namespace Witcher3StringEditor.Integrations.Terminology;

public sealed record TerminologyEntry(
    string Term,
    string Translation,
    string? Notes,
    string? Mode);

public sealed record TerminologyPack(
    string Name,
    string SourcePath,
    IReadOnlyList<TerminologyEntry> Entries);

public sealed class StyleGuideSection
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> Rules { get; init; } = new List<string>();
}

public sealed record StyleGuide(
    string Name,
    string SourcePath,
    IReadOnlyList<StyleGuideSection> Sections,
    IReadOnlyList<string> RequiredTerms,
    IReadOnlyList<string> ForbiddenTerms,
    IReadOnlyList<string> ToneNotes);
