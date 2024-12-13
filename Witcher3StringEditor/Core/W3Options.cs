using CommandLine;

namespace Witcher3StringEditor.Core
{
    internal class W3Options
    {
        [Option('d')]
        public string Decode { get; set; } = string.Empty;

        [Option('e')]
        public string Encode { get; set; } = string.Empty;

        [Option('i')]
        public int IdSpace { get; set; }

        [Option("force-ignore-id-space-check-i-know-what-i-am-doing")]
        public bool IsIgnoreIdSpaceCheck { get; set; }
    }
}