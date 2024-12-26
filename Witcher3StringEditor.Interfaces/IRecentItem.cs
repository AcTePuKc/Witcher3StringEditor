namespace Witcher3StringEditor.Interfaces;

public interface IRecentItem
{
    public string FilePath { get; }

    public DateTime OpenedTime { get; set; }

    public bool IsMarked { get; set; }
}