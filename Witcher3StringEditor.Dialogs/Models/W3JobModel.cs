using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Shared;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Dialogs.Models;

internal partial class W3JobModel : ObservableObject, IW3Job
{
    [ObservableProperty] private int idSpace;

    [ObservableProperty] private bool isIgnoreIdSpaceCheck;

    [ObservableProperty] private W3Language language;
    [ObservableProperty] private W3FileType w3FileType;

    public required string Path { get; set; }

    public required IEnumerable<IW3Item> W3Items { get; set; }
}