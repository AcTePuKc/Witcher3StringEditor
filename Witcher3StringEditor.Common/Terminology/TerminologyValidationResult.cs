namespace Witcher3StringEditor.Common.Terminology;

public sealed record TerminologyValidationResult(bool IsValid, string Message)
{
    public static TerminologyValidationResult NotValidated { get; } = new(true, "Not validated.");
}
