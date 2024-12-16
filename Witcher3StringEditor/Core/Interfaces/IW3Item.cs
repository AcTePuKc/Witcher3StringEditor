namespace Witcher3StringEditor.Core.Interfaces
{
    public interface IW3Item
    {
        public string StrID { get; set; }

        public string KeyHex { get; set; }

        public string KeyName { get; set; }

        public string Text { get; set; }
    }
}