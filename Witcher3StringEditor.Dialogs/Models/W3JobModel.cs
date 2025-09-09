using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Dialogs.Models;

internal partial class W3JobModel : ObservableObject, IW3Job
{
    [ObservableProperty] private int _idSpace;

    [ObservableProperty] private bool _isIgnoreIdSpaceCheck;

    [ObservableProperty] private W3Language _language;
    [ObservableProperty] private W3FileType _w3FileType;

    public required string Path { get; init; }

    public required IEnumerable<IW3Item> W3Items { get; init; }
}