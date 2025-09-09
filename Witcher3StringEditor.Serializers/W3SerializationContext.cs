using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Serializers;

public class W3SerializationContext
{
    public string Output { get; set; } = string.Empty;

    public W3FileType FileType { get; set; }

    public W3Language Language { get; set; }

    public int IdSpace { get; set; }

    public bool IsIgnoreIdSpaceCheck { get; set; }
}