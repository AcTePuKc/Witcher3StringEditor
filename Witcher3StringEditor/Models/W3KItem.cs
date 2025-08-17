using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models
{
    internal class W3KItem : IW3KItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public required string Text { get; set; }

        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
