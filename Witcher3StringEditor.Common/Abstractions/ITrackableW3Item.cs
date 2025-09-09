namespace Witcher3StringEditor.Common.Abstractions;

public interface IEditableW3StringItem : IW3StringItem, ICloneable
{
    public Guid Id { get; }
}