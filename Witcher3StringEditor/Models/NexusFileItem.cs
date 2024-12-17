namespace Witcher3StringEditor.Models;

public record NexusFileItem(Version Version)
{
    public Version Version { get; } = Version;
}