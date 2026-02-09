using System.Collections.Generic;

namespace Witcher3StringEditor.Common.QualityAssurance;

public enum QualityAssuranceSeverity
{
    Info,
    Warning,
    Error
}

public sealed record QualityAssuranceMessageDto(
    IReadOnlyList<string> Messages,
    QualityAssuranceSeverity Severity,
    string SourceFile);
