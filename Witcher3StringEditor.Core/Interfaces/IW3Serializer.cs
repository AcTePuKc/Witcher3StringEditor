namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IW3Serializer
    {
        public Task<IEnumerable<IW3Item>> Deserialize(string path);

        public Task<bool> Serialize(IW3Job w3Job);
    }
}