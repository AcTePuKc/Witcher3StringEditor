using CommunityToolkit.Mvvm.ComponentModel;
using MiniExcelLibs.Attributes;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class W3Item : ObservableObject, IW3Item
{
    [ObservableProperty] private string keyHex = string.Empty;

    [ObservableProperty] private string keyName = string.Empty;

    [ObservableProperty] private string oldText = string.Empty;

    [ObservableProperty] private string strId = string.Empty;

    [ObservableProperty] private string text = string.Empty;

    [ExcelIgnore] public Guid Id { get; } = Guid.NewGuid();

    public object Clone()
    {
        return MemberwiseClone();
    }
}