namespace Witcher3StringEditor.Interfaces;

public interface IKnowledgeService
{
    public  Task<IW3KItem> Learn(string text);

    public  IAsyncEnumerable<IW3KItem>? Search(string text, int count);

    public IAsyncEnumerable<IW3KItem>? All();

    public  Task Delete(IEnumerable<string> items);

    public  Task Clear();
}
