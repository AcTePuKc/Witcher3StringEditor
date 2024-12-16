using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core.Implements;

internal class W3Item : IW3Item
{
    public string StrId { get; init; } = string.Empty;
    public string KeyHex { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}