using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Serializers.Abstractions;

public interface IW3Serializer
{
    public Task<IReadOnlyList<IW3Item>> Deserialize(string filePath);

    public Task<bool> Serialize(IReadOnlyList<IW3Item> w3Items, W3SerializationContext context);
}