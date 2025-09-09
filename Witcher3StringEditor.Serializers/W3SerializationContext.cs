using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Serializers;

public record W3SerializationContext
{
    public required string OutputDirectory { get; init; } 

    public required W3FileType TargetFileType { get; init; }

    public required W3Language  TargetLanguage { get; init; }

    public required int ExpectedIdSpace { get; init; }

    public bool IgnoreIdSpaceCheck { get; init; }
}