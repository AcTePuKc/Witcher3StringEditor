using CommandLine;
using JetBrains.Annotations;

namespace Witcher3StringEditor.Serializers.Internal;

internal record W3StringsOptions
{
    [UsedImplicitly] [Option('d')] public string InputFileToDecode { get; init; } = string.Empty;

    [UsedImplicitly] [Option('e')] public string InputFileToEncode { get; init; } = string.Empty;

    [UsedImplicitly] [Option('i')] public int ExpectedIdSpace { get; init; }

    [UsedImplicitly]
    [Option("force-ignore-id-space-check-i-know-what-i-am-doing")]
    public bool IgnoreIdSpaceCheck { get; init; }
}