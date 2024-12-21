using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Dialogs.Models
{
    public partial class RecentItem : ObservableObject, IRecentItem
    {
        [ObservableProperty]
        private string filePath;

        [ObservableProperty]
        private DateTime openedTime;

        [ObservableProperty]
        private bool isPin;

        public RecentItem(string filePath, DateTime openedTime, bool isPin = false)
        {
            IsPin = isPin;
            FilePath = filePath;
            OpenedTime = openedTime;
        }
    }
}
