using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class W3Job : ObservableObject, IW3Job
{
    [ObservableProperty] private FileType fileType;

    [ObservableProperty] private int idSpace;

    [ObservableProperty] private bool isIgnoreIdSpaceCheck;

    [ObservableProperty] private W3Language language;

    public required string Path { get; init; }

    public required IEnumerable<IW3Item> W3Items { get; init; }
}