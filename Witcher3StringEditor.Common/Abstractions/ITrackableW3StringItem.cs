namespace Witcher3StringEditor.Common.Abstractions;

public interface ITrackableW3StringItem : IW3StringItem, ICloneable
{
    public Guid Id { get; }
}