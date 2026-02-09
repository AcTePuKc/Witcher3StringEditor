using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Terminology;

public sealed class TerminologyEntry
{
    public string Term { get; init; } = string.Empty;

    public string Translation { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public string? Mode { get; init; }
}

public sealed class TerminologyPack
{
    public string Name { get; init; } = string.Empty;

    public string SourcePath { get; init; } = string.Empty;

    public IReadOnlyList<TerminologyEntry> Entries { get; init; } = new List<TerminologyEntry>();
}

public sealed class StyleGuide
{
    public string Name { get; init; } = string.Empty;

    public string SourcePath { get; init; } = string.Empty;

    public IReadOnlyList<StyleGuideSection> Sections { get; init; } = new List<StyleGuideSection>();

    public IReadOnlyList<string> RequiredTerms { get; init; } = new List<string>();

    public IReadOnlyList<string> ForbiddenTerms { get; init; } = new List<string>();

    public IReadOnlyList<string> ToneNotes { get; init; } = new List<string>();
}

public sealed class StyleGuideSection
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> Rules { get; init; } = new List<string>();
}
