namespace Witcher3StringEditor.Interfaces;

public interface IModelSettings
{
    public string EndPoint { get; set; }

    public string ModelId { get; set; }

    public string ApiKey { get; set; }

    public string Prompts { get; set; }
}