namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IRecentItem
    {
        public string FilePath { get; }

        public DateTime OpenedTime { get; set; }

        public bool IsPin { get; set; }
    }
}