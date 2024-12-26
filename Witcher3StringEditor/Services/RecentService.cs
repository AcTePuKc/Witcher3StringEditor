using System.IO;
using System.Text.Json;
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
        File.WriteAllText(recentFilesPath, JsonSerializer.Serialize(recentItems));
    }

    public IEnumerable<IRecentItem> GetRecentItems()
    {
        if (!File.Exists(recentFilesPath)) return [];
        return JsonSerializer.Deserialize<IEnumerable<RecentItem>>(File.ReadAllText(recentFilesPath)) ?? [];
    }
}