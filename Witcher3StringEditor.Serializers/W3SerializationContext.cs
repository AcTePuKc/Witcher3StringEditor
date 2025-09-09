using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Serializers;

public record W3SerializationContext
{
    public required string OutputDirectory { get; set; } 

    public required W3FileType TargetFileType { get; set; }

    public required W3Language  TargetLanguage { get; set; }

    public required int ExpectedIdSpace { get; set; }

    public bool IgnoreIdSpaceCheck { get; set; }
}