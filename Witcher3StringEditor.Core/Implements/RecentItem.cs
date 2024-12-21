using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core.Implements
{
    internal class RecentItem(string fileName, DateTime openedTime, bool isPin) : IRecentItem
    {
        public string FileName { get; } = fileName;

        public DateTime OpenedTime { get; set; } = openedTime;

        public bool IsPin { get; set; } = isPin;
    }
}