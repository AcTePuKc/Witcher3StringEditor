using Newtonsoft.Json;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core;

public class RecentManger
{
    private readonly string recentFilesPath;

    private static readonly Lazy<RecentManger> LazyInstance
    = new(static () => new RecentManger("RecentFiles.json"));

    public static RecentManger Instance => LazyInstance.Value;

    private RecentManger(string path)
    {
        recentFilesPath = path;
    }

    public void Update(IEnumerable<IRecentItem> recentItems)
    {
        File.WriteAllText(recentFilesPath, JsonConvert.SerializeObject(recentItems));
    }

    public IEnumerable<IRecentItem> GetRecentItems()
    {
        if (!File.Exists(recentFilesPath)) return [];
        var json = File.ReadAllText(recentFilesPath);
        return JsonConvert.DeserializeObject<IEnumerable<RecentItem>>(json) ?? [];
    }
}