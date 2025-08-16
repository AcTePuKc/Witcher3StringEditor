namespace Witcher3StringEditor.Interfaces;

public interface IEmbeddedModelSettings
{
    public string EndPoint { get; set; }

    public string ModelId { get; set; }

    public string ApiKey { get; set; }

    public int Dimensions { get; set; }
}