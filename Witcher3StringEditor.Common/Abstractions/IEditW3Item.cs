namespace Witcher3StringEditor.Common.Abstractions;

public interface IEditW3Item : IW3Item, ICloneable
{
    public Guid Id { get; }
}