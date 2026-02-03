using System;

namespace Witcher3StringEditor.Common.QualityAssurance;

public sealed class QualityAssuranceEntry
{
    public string SourceText { get; init; } = string.Empty;

    public string TargetText { get; init; } = string.Empty;

    public string IssueType { get; init; } = string.Empty;

    public string? Details { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class QualityAssuranceQuery
{
    public string SourceText { get; init; } = string.Empty;

    public string TargetText { get; init; } = string.Empty;

    public string IssueType { get; init; } = string.Empty;
}
