namespace Witcher3StringEditor.Interfaces
{
    public interface IW3KItem
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
