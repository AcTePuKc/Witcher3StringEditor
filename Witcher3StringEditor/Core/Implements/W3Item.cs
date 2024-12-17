using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core.Implements;

public record W3Item : IW3Item
{
    public required string StrId { get; init; }
    public required string KeyHex { get; init; }
    public required string KeyName { get; init; }
    public required string Text { get; init; }
}