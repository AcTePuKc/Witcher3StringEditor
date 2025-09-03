using JetBrains.Annotations;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Serializers.Internal;

internal class W3Item : IW3Item
{
    public W3Item()
    {
    }

    [UsedImplicitly]
    public W3Item(IW3Item w3Item)
    {
        StrId = w3Item.StrId;
        KeyHex = w3Item.KeyHex;
        KeyName = w3Item.KeyName;
        OldText = w3Item.OldText;
        Text = w3Item.Text;
    }

    public string StrId { get; set; } = string.Empty;

    public string KeyHex { get; set; } = string.Empty;

    public string KeyName { get; set; } = string.Empty;

    public string OldText { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}