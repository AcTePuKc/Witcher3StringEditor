using AngleSharp;
using Serilog;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class CheckUpdateService : ICheckUpdateService
{
    private const string Address = "https://www.nexusmods.com/witcher3/mods/10032";
    private const string Selectors = "#pagetitle>ul.stats.clearfix>li.stat-version>div>div.stat";

    public async Task<bool> CheckUpdate()
    {
        try
        {
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(Address);
            var element = document.QuerySelector(Selectors);
            return element != null
                && new Version(element.InnerHtml) > new Version(ThisAssembly.AssemblyFileVersion);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to check for updates: {ex.Message}");
            return false;
        }
    }
}