using Witcher3StringEditor.Common;

namespace Witcher3StringEditor.Abstractions;

public interface IW3Job
{
    public W3Language Language { get; set; }

    public int IdSpace { get; set; }

    public bool IsIgnoreIdSpaceCheck { get; set; }

    public W3FileType W3FileType { get; set; }

    public string Path { get; set; }

    public IEnumerable<IW3Item> W3Items { get; set; }
}