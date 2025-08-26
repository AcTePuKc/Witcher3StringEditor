namespace Witcher3StringEditor.Abstractions;

public interface IRecentItem
{
    public string FilePath { get; }

    public DateTime OpenedTime { get; set; }

    public bool IsMarked { get; set; }
}