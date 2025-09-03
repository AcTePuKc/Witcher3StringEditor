using JetBrains.Annotations;

namespace Witcher3StringEditor.Common.Abstractions;

public interface IW3Item
{
    public string StrId { get; set; }

    public string KeyHex { get; set; }

    public string KeyName { get; set; }

    [UsedImplicitly] public string OldText { get; set; }

    public string Text { get; set; }
}