using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Dialogs.Models
{
    partial class RecentItem : ObservableObject, IRecentItem
    {
        public string FileName => Path.GetFileName(FileName);

        [ObservableProperty]
        private string filePath;

        [ObservableProperty]
        private DateTime openedTime;

        [ObservableProperty]
        private bool isPin;

        public RecentItem(string filePath, DateTime openedTime, bool isPin)
        {
            IsPin = isPin;
            FilePath = filePath;
            OpenedTime = openedTime;
        }
    }
}
