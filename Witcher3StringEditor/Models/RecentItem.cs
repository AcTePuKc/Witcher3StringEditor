using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class RecentItem : ObservableObject, IRecentItem
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