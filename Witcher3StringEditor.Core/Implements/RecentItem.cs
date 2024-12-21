using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core.Implements
{
    internal record RecentItem: IRecentItem
    {
        public required string FilePath { get; set; }

        public required DateTime OpenedTime { get; set; }

        public required bool IsPin { get; set; }
    }
}