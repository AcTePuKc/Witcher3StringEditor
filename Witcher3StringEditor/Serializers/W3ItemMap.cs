using CsvHelper.Configuration;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Serializers;

internal sealed class W3ItemMap:ClassMap<W3Item>
{
    public W3ItemMap()
    {
        Map(m => m.Id).Ignore();
        Map(m => m.StrId).Index(0);
        Map(m => m.KeyHex).Index(1);
        Map(m => m.KeyName).Index(2);
        Map(m => m.OldText).Ignore();
        Map(m => m.Text).Index(3);
    }
}