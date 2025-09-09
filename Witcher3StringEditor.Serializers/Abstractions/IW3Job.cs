using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Serializers.Abstractions;

public interface IW3Job
{
    public W3Language Language { get; set; }

    public int IdSpace { get; set; }

    public bool IsIgnoreIdSpaceCheck { get; set; }

    public W3FileType W3FileType { get; set; }

    public string Path { get; }

    public IReadOnlyCollection<IW3Item> W3Items { get; }
}