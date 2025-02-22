using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class W3Job : ObservableObject, IW3Job
{
    [ObservableProperty] private int idSpace;

    [ObservableProperty] private bool isIgnoreIdSpaceCheck;

    [ObservableProperty] private W3Language language;
    [ObservableProperty] private W3FileType w3FileType;

    public required string Path { get; init; }

    public required IEnumerable<IW3Item> W3Items { get; init; }
}