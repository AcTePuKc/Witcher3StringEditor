using CommunityToolkit.Mvvm.ComponentModel;
using MiniExcelLibs.Attributes;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

internal partial class W3Item : ObservableObject, IW3Item
{
    [ExcelIgnore]
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string strId = string.Empty;

    [ObservableProperty]
    private string keyHex = string.Empty;

    [ObservableProperty]
    private string keyName = string.Empty;

    [ObservableProperty]
    [ExcelColumnWidth(50)]
    private string oldText = string.Empty;

    [ObservableProperty]
    [ExcelColumnWidth(50)]
    private string text = string.Empty;

    public object Clone() => MemberwiseClone();
}