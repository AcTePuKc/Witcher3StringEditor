using CommandLine;

namespace Witcher3StringEditor.Core
{
    internal record W3Options
    {
        [Option('d')]
        public string Decode { get; init; } = string.Empty;

        [Option('e')]
        public string Encode { get; init; } = string.Empty;

        [Option('i')]
        public int IdSpace { get; init; }

        [Option("force-ignore-id-space-check-i-know-what-i-am-doing")]
        public bool IsIgnoreIdSpaceCheck { get; init; }
    }
}