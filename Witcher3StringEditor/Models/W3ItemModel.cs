using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Abstractions;

namespace Witcher3StringEditor.Models;

public partial class W3ItemModel : ObservableObject, IW3Item
{
    [ObservableProperty] private string keyHex = string.Empty;

    [ObservableProperty] private string keyName = string.Empty;

    [ObservableProperty] private string oldText = string.Empty;

    [ObservableProperty] private string strId = string.Empty;

    [ObservableProperty] private string text = string.Empty;

    public W3ItemModel(IW3Item w3Item)
    {
        Id = w3Item.Id;
        StrId = w3Item.StrId;
        KeyHex = w3Item.KeyHex;
        KeyName = w3Item.KeyName;
        OldText = w3Item.OldText;
        Text = w3Item.Text;
    }

    public W3ItemModel()
    {
    }

    public Guid Id { get; } = Guid.NewGuid();

    public object Clone()
    {
        return MemberwiseClone();
    }

    partial void OnTextChanging(string value)
    {
        if (string.IsNullOrWhiteSpace(OldText)) OldText = Text;
    }
}