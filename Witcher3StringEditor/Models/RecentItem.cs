using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class RecentItem : ObservableObject, IRecentItem
{
    [ObservableProperty] private string filePath;

    [ObservableProperty] private bool isMarked;

    [ObservableProperty] private DateTime openedTime;

    public RecentItem(string filePath, DateTime openedTime, bool isMarked = false)
    {
        IsMarked = isMarked;
        FilePath = filePath;
        OpenedTime = openedTime;
    }
}