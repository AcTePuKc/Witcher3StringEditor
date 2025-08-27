namespace Witcher3StringEditor.Shared.Abstractions;

public interface IEditW3Item : IW3Item, ICloneable
{
    public Guid Id { get; }
}