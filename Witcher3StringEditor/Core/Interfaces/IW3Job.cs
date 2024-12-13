using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IW3Job
    {
        public W3Language Language { get; set; }

        public int IDSpace { get; set; }

        public bool IsIgnoreIDSpaceCheck { get; set; }

        public FileType FileType { get; set; }

        public string Path { get; set; }

        public IEnumerable<W3ItemModel> W3Items { get; set; }
    }
}