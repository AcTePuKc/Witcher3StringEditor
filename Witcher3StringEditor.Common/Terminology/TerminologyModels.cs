using System.Collections.Generic;

namespace Witcher3StringEditor.Common.Terminology;

public sealed class TerminologyEntry
{
    public string SourceTerm { get; init; } = string.Empty;

    public string TargetTerm { get; init; } = string.Empty;

    public string? Notes { get; init; }
}

public sealed class TerminologyPack
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<TerminologyEntry> Entries { get; init; } = new List<TerminologyEntry>();
}
