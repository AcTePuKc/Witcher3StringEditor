namespace Witcher3StringEditor.Abstractions;

public interface IEditW3Item : IW3Item, ICloneable
{
    public Guid Id { get; }
}
