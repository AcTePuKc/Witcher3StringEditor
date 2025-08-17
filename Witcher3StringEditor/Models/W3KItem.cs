using MessagePack;
using MessagePack.Formatters;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models
{
    [MessagePackObject(AllowPrivate = true)]
    internal class W3KItem : IW3KItem
    {
        [Key(0)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Key(1)]
        public required string Text { get; set; }

        [Key(2)]
        [MessagePackFormatter(typeof(ReadOnlyMemoryFormatter<float>))]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
