namespace Witcher3StringEditor.Common.QualityAssurance;

public sealed record QualityAssuranceDiagnosticDto(
    string Code,
    string Message,
    QualityAssuranceSeverity Severity,
    string Location);
