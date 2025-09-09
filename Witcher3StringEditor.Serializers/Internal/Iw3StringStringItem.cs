using JetBrains.Annotations;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Serializers.Internal;

internal class Iw3StringStringItem : IW3StringItem
{
    public Iw3StringStringItem()
    {
    }

    [UsedImplicitly]
    public Iw3StringStringItem(IW3StringItem iw3StringItem)
    {
        StrId = iw3StringItem.StrId;
        KeyHex = iw3StringItem.KeyHex;
        KeyName = iw3StringItem.KeyName;
        OldText = iw3StringItem.OldText;
        Text = iw3StringItem.Text;
    }

    public string StrId { get; set; } = string.Empty;

    public string KeyHex { get; set; } = string.Empty;

    public string KeyName { get; set; } = string.Empty;

    public string OldText { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}