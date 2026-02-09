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

public sealed record StyleGuideSection(
    string Name,
    IReadOnlyList<string> Rules);

public sealed record StyleGuide(
    string Name,
    string SourcePath,
    IReadOnlyList<StyleGuideSection> Sections,
    IReadOnlyList<string> RequiredTerms,
    IReadOnlyList<string> ForbiddenTerms,
    IReadOnlyList<string> ToneNotes);
