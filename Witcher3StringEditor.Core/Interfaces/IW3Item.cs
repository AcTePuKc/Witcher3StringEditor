namespace Witcher3StringEditor.Core.Interfaces;

public interface IW3Item
{
    public string StrId { get; }

    public string KeyHex { get; }

    public string KeyName { get; }

    public string Text { get; }
}