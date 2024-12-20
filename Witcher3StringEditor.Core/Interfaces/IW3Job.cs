using Witcher3StringEditor.Core.Common;

namespace Witcher3StringEditor.Core.Interfaces;

public interface IW3Job
{
    public W3Language Language { get; set; }

    public int IdSpace { get; set; }

    public bool IsIgnoreIdSpaceCheck { get; set; }

    public FileType FileType { get; set; }

    public string Path { get; }

    public IEnumerable<IW3Item> W3Items { get; }
}