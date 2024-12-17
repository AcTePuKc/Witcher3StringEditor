namespace Witcher3StringEditor.Models;

public record NexusResponse(List<NexusFileItem> Files)
{
    public List<NexusFileItem> Files { get; } = Files;
}