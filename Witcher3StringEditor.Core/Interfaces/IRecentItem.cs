namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IRecentItem
    {
        public string FileName { get; }

        public DateTime OpenedTime { get; }

        public bool IsPin { get; set; }
    }
}