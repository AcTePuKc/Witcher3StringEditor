using AngleSharp;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class CheckUpdateService(IAppSettings appSettings) : ICheckUpdateService
{
    private const string Selectors = "li.stat-version>div>div.stat";
    private readonly string address = appSettings.NexusModUrl;

    public async Task<bool> CheckUpdate()
    {
        try
        {
            using var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            using var document = await context.OpenAsync(address);
            var element = document.QuerySelector(Selectors);
            Guard.IsNotNull(element);
            Guard.IsTrue(Version.TryParse(element.InnerHtml, out var lastestVersion));
            Guard.IsTrue(Version.TryParse(ThisAssembly.AssemblyFileVersion, out var currentVersion));
            Guard.IsNotNull(lastestVersion);
            Guard.IsNotNull(currentVersion);
            return lastestVersion > currentVersion;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates.");
            return false;
        }
    }
}