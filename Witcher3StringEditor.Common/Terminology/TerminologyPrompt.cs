namespace Witcher3StringEditor.Common.Terminology;

public sealed record TerminologyPrompt
{
    public string? SystemPrompt { get; init; }

    public string? UserPrompt { get; init; }
}
