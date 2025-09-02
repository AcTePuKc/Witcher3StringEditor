using JetBrains.Annotations;
using Witcher3StringEditor.Shared;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Serializers.Abstractions;

public interface IW3Job
{
    public W3Language Language { get; set; }

    public int IdSpace { get; set; }

    public bool IsIgnoreIdSpaceCheck { get; set; }

    public W3FileType W3FileType { get; set; }

    public string Path { get; }

    public IEnumerable<IW3Item> W3Items { get; }
}