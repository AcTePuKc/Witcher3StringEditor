using JetBrains.Annotations;

namespace Witcher3StringEditor.Shared.Abstractions;

public interface IRecentItem
{
    public string FilePath { get; }

    [UsedImplicitly] public DateTime OpenedTime { get; set; }

    public bool IsMarked { get; set; }
}