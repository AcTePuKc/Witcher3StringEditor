using System.Net.Http;
using System.Text;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    private readonly Uri updateUrl = new("https://api.nexusmods.com/v2/graphql");

    /// <summary>
    ///     Checks if an update is available for the application
    /// </summary>
    /// <returns>True if an update is available, false otherwise or if an error occurred</returns>
    public async Task<bool> CheckUpdate()
    {
        try
        {
            Log.Information("Checking for updates..."); // Log start of update check
            var lastestVersion = await FetchLatestVersion(); // Fetch latest version
            var currentVersion = GetCurrentVersion(); // Get current version
            var isUpdateAvailable = lastestVersion > currentVersion; // Compare versions
            Log.Information(
                "Update check completed. Current version: {CurrentVersion}, Latest version: {LatestVersion}, Update available: {IsUpdateAvailable}",
                currentVersion, lastestVersion, isUpdateAvailable); // Log update check results
            return isUpdateAvailable; // Return update availability
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates."); // Log any errors
            return false; // Return false on failure
        }
    }

    /// <summary>
    ///     Gets the current application version from assembly file version
    /// </summary>
    /// <returns>The current application version</returns>
    /// <exception cref="InvalidOperationException">Thrown when version parsing fails</exception>
    private static Version GetCurrentVersion()
    {
        Guard.IsTrue(Version.TryParse(ThisAssembly.AssemblyFileVersion,
            out var currentVersion)); // Parse current version
        Guard.IsNotNull(currentVersion); // Ensure current version is not null
        return currentVersion; // Return current version
    }

    /// <summary>
    ///     Fetches the latest version information from NexusMods API
    /// </summary>
    /// <returns>The latest available version</returns>
    private async Task<Version> FetchLatestVersion()
    {
        var response = await SendGraphQlRequest(); // Send GraphQL request
        return ExtractVersionFromResponse(response); // Extract version from response
    }

    /// <summary>
    ///     Extracts version information from GraphQL API response
    /// </summary>
    /// <param name="response">The JSON response string from API</param>
    /// <returns>Parsed version from the response</returns>
    /// <exception cref="InvalidOperationException">Thrown when response parsing fails</exception>
    private static Version ExtractVersionFromResponse(string response)
    {
        var jObject = JObject.Parse(response); // Parse response
        var nodes = jObject["data"]?["mods"]?["nodes"]?.ToArray(); // Get nodes array
        Guard.IsNotNull(nodes); // Ensure nodes array is not null
        Guard.IsNotEmpty(nodes); // Ensure nodes array is not empty
        var versionToken = nodes[0]["version"]; // Get version token
        Guard.IsNotNull(versionToken); // Ensure version token is not null
        var versionString = versionToken.Value<string>(); // Extract version string
        Guard.IsNotNullOrWhiteSpace(versionString); // Ensure version string is not empty
        Guard.IsTrue(Version.TryParse(versionString, out var version)); // Parse version string
        Guard.IsNotNull(version); // Ensure version is not null
        return version; // Return version
    }

    /// <summary>
    ///     Sends GraphQL request to NexusMods API to fetch mod information
    /// </summary>
    /// <returns>Raw JSON response string from the API</returns>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    private async Task<string> SendGraphQlRequest()
    {
        using var httpClient = new HttpClient(); // Create HTTP client
        var body = JsonConvert.SerializeObject(new
        {
            query = @"query mods($filter: ModsFilter){mods(filter: $filter){nodes {version}nodesCount}}",
            variables = new
            {
                filter = new
                {
                    name = new
                    {
                        value = "The Witcher3 String Editor NextGen",
                        op = "EQUALS"
                    }
                }
            }
        }); // Create request body
        using var content = new StringContent(body, Encoding.UTF8, "application/json"); // Create string content
        using var response = await httpClient.PostAsync(updateUrl, content); // Send request
        Guard.IsTrue(response.IsSuccessStatusCode); // Ensure request was successful
        return await response.Content.ReadAsStringAsync(); // Read response
    }
}