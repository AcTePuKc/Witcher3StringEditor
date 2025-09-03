using System.Net.Http;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

internal class CheckUpdateService : ICheckUpdateService
{
    private readonly Uri updateUrl = new($"https://witcher3stringeditorcheckupdate.azurewebsites.net/api/checkupdate"); 

    public async Task<bool> CheckUpdate()
    {
        try
        {
            Log.Information("Checking for updates...");
            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(updateUrl);
            if (!httpResponse.IsSuccessStatusCode) return true;
            Guard.IsTrue(Version.TryParse(await httpResponse.Content.ReadAsStringAsync(), out var lastestVersion));
            Guard.IsTrue(Version.TryParse(ThisAssembly.AssemblyFileVersion, out var currentVersion));
            Guard.IsNotNull(lastestVersion);
            Guard.IsNotNull(currentVersion);
            var isUpdateAvailable = lastestVersion > currentVersion;
            Log.Information(
                "Update check completed. Current version: {CurrentVersion}, Latest version: {LatestVersion}, Update available: {IsUpdateAvailable}",
                currentVersion, lastestVersion, isUpdateAvailable);
            return isUpdateAvailable;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates.");
            return false;
        }
    }
}