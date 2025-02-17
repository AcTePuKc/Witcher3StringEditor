using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class W3Item : ObservableObject, IW3Item
{
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string keyHex = string.Empty;

    [ObservableProperty]
    private string keyName = string.Empty;

    [ObservableProperty]
    private string strId = string.Empty;

    [ObservableProperty]
    private string oldText = string.Empty;

    [ObservableProperty]
    private string text = string.Empty;

    public object Clone() => MemberwiseClone();
}