namespace Witcher3StringEditor.Common.Abstractions;

public interface IEditableW3Item : IW3Item, ICloneable
{
    public Guid Id { get; }
}