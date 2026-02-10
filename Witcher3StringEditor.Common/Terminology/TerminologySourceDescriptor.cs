namespace Witcher3StringEditor.Common.Terminology;

public enum TerminologySourceType
{
    TerminologyPack,
    StyleGuide
}

public sealed record TerminologySourceDescriptor(string Path, TerminologySourceType SourceType);
