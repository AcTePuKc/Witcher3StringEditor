using Newtonsoft.Json;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core;

public class RecentManger
{
    private readonly string recentFilesPath;
    private readonly IEnumerable<IRecentItem> recentItems;

    private static readonly Lazy<RecentManger> LazyInstance
    = new(static () => new RecentManger("RecentFiles.json"));

    public static RecentManger Instance => LazyInstance.Value;

    private RecentManger(string path)
    {
        recentFilesPath = path;
        recentItems = GetRecentItems(recentFilesPath);
    }

    public void Add(IRecentItem recentItem)
        => Update(recentItems.Append(recentItem));

    public void Delete(IEnumerable<IRecentItem> items)
    {
        var list = recentItems.ToList();
        foreach (var item in items)
            list.Remove(item);
        Update(list);
    }

    public void Update(IEnumerable<IRecentItem> recentItems)
    {
        File.WriteAllText(recentFilesPath, JsonConvert.SerializeObject(recentItems));
    }

    private static IEnumerable<IRecentItem> GetRecentItems(string path)
    {
        if (!File.Exists(path)) return [];
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<IEnumerable<RecentItem>>(json) ?? [];
    }
}