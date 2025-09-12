using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Serializers.Abstractions;

public interface IW3Serializer
{
    public Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath);

    public Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context);
}