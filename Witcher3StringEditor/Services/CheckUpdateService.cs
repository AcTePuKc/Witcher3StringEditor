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
            Log.Information("Checking for updates..."); // Log start of update check
            using var httpClient = new HttpClient(); // Create HTTP client
            var httpResponse = await httpClient.GetAsync(updateUrl); // Send GET request to update URL
            if (!httpResponse.IsSuccessStatusCode) return true; // Return true if request failed
            Guard.IsTrue(Version.TryParse(await httpResponse.Content.ReadAsStringAsync(),
                out var lastestVersion)); // Parse latest version
            Guard.IsTrue(Version.TryParse(ThisAssembly.AssemblyFileVersion,
                out var currentVersion)); // Parse current version
            Guard.IsNotNull(lastestVersion); // Ensure latest version is not null
            Guard.IsNotNull(currentVersion); // Ensure current version is not null
            var isUpdateAvailable = lastestVersion > currentVersion; // Compare versions
            Log.Information( // Log update check results
                "Update check completed. Current version: {CurrentVersion}, Latest version: {LatestVersion}, Update available: {IsUpdateAvailable}",
                currentVersion, lastestVersion, isUpdateAvailable);
            return isUpdateAvailable; // Return update availability
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates."); // Log any errors
            return false; // Return false on failure
        }
    }
}