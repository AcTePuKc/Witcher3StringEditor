namespace Witcher3StringEditor.Common.TranslationMemory;

public sealed class TranslationMemorySettings
{
    public bool IsEnabled { get; init; }

    public string? DatabasePath { get; init; }

    public string? ProviderName { get; init; }
}
