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

    public string Path { get; init; } = string.Empty;

    public IEnumerable<W3ItemModel> W3Items { get; set; } = [];
}