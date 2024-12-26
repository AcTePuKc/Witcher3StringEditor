using Newtonsoft.Json;
using System.IO;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class RecentService : IRecentService
{
    private readonly string recentFilesPath;

    private static readonly Lazy<RecentService> LazyInstance
    = new(static () => new RecentService("RecentFiles.json"));

    public static RecentService Instance => LazyInstance.Value;

    private RecentService(string path)
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