namespace Witcher3StringEditor.Abstractions;

public interface IW3Serializer
{
    public Task<IEnumerable<IW3Item>> Deserialize(string path);

    public Task<bool> Serialize(IW3Job w3Job);
}