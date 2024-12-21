using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core.Implements
{
    internal class RecentItem(string path, DateTime openedTime, bool isPin) : IRecentItem
    {
        public string FileName => Path.GetFileName(FilePath);

        public string FilePath { get; } = path;

        public DateTime OpenedTime { get; set; } = openedTime;

        public bool IsPin { get; set; } = isPin;
    }
}