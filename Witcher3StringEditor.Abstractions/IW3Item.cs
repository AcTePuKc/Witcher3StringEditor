namespace Witcher3StringEditor.Abstractions;

public interface IW3Item
{
    public Guid Id { get; }

    public string StrId { get; set; }

    public string KeyHex { get; set; }

    public string KeyName { get; set; }

    public string OldText { get; set; }

    public string Text { get; set; }

    object Clone();
}