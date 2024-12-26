namespace Witcher3StringEditor.Core.Interfaces;

public interface IRecentService
{
    public void Update(IEnumerable<IRecentItem> recentItems);

    public IEnumerable<IRecentItem> GetRecentItems();
}