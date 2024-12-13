using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models
{
    internal partial class W3Job : ObservableObject, IW3Job
    {
        [ObservableProperty]
        private W3Language language;

        [ObservableProperty]
        private int iDSpace;

        [ObservableProperty]
        private bool isIgnoreIDSpaceCheck;

        [ObservableProperty]
        private FileType fileType;

        public string Path { get; set; } = string.Empty;

        public IEnumerable<W3ItemModel> W3Items { get; set; } = [];
    }
}