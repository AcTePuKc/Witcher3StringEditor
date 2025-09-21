using System.Net.Http;
using CommunityToolkit.Diagnostics;
using Serilog;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides update checking functionality
///     Implements the ICheckUpdateService interface to handle checking for application updates
/// </summary>
internal class CheckUpdateService : ICheckUpdateService
{
    /// <summary>
    ///     The URL to check for updates
    /// </summary>
    private readonly Uri updateUrl = new("https://witcher3stringeditorcheckupdate.azurewebsites.net/api/checkupdate");

    /// <summary>
    ///     Checks if an update is available for the application
    /// </summary>
    /// <returns>True if an update is available, false otherwise or if an error occurred</returns>
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