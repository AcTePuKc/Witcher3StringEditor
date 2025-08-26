using CsvHelper.Configuration.Attributes;
using Witcher3StringEditor.Abstractions;

namespace Witcher3StringEditor.Serializers;

internal class W3Item : IW3Item
{
    public W3Item()
    {
    }

    public W3Item(IW3Item w3Item)
    {
        Id = w3Item.Id;
        StrId = w3Item.StrId;
        KeyHex = w3Item.KeyHex;
        KeyName = w3Item.KeyName;
        OldText = w3Item.OldText;
        Text = w3Item.Text;
    }

    [Ignore] public Guid Id { get; } = Guid.NewGuid();

    public string StrId { get; set; } = string.Empty;

    public string KeyHex { get; set; } = string.Empty;

    public string KeyName { get; set; } = string.Empty;

    [Ignore] public string OldText { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public object Clone()
    {
        return MemberwiseClone();
    }
}