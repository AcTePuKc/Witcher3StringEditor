using System.Net.Http;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Services;

internal class CheckUpdateService(ILogger<CheckUpdateService> logger) : ICheckUpdateService
{
    private static string UpdateUrl => "https://witcher3stringeditorcheckupdate.azurewebsites.net/api/checkupdate";

    public async Task<bool> CheckUpdate()
    {
        try
        {
            logger.LogInformation("Checking for updates...");
            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(UpdateUrl);
            if (!httpResponse.IsSuccessStatusCode) return true;
            Guard.IsTrue(Version.TryParse(await httpResponse.Content.ReadAsStringAsync(), out var lastestVersion));
            Guard.IsTrue(Version.TryParse(ThisAssembly.AssemblyFileVersion, out var currentVersion));
            Guard.IsNotNull(lastestVersion);
            Guard.IsNotNull(currentVersion);
            var isUpdateAvailable = lastestVersion > currentVersion;
            logger.LogInformation(
                "Update check completed. Current version: {CurrentVersion}, Latest version: {LatestVersion}, Update available: {IsUpdateAvailable}",
                currentVersion, lastestVersion, isUpdateAvailable);
            return isUpdateAvailable;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check for updates.");
            return false;
        }
    }
}