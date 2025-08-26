namespace Witcher3StringEditor.Abstractions;

public interface IEditW3Item : IW3Item
{
    public Guid Id { get; }

    object Clone();
}
