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
            using var httpClient = new HttpClient(); // Create HTTP client
            const string graphqlQuery =
                @"query mods($filter: ModsFilter){mods(filter: $filter){nodes {version}nodesCount}}"; // Create GraphQL query
            var variables = new
            {
                filter = new
                {
                    name = new
                    {
                        value = "The Witcher3 String Editor NextGen",
                        op = "EQUALS"
                    }
                }
            }; // Create variables
            var requestBody =
                JsonConvert.SerializeObject(new { query = graphqlQuery, variables }); // Create request body
            var stringContent =
                new StringContent(requestBody, Encoding.UTF8, "application/json"); // Create string content
            var httpResponse = await httpClient.PostAsync(updateUrl, stringContent); // Send request
            Guard.IsTrue(httpResponse.IsSuccessStatusCode); // Ensure request was successful
            var jsonString = await httpResponse.Content.ReadAsStringAsync(); // Read response content as string
            var jObject = JObject.Parse(jsonString); // Parse JSON
            var nodes = jObject["data"]?["mods"]?["nodes"]?.ToArray(); // Get nodes
            Guard.IsNotNull(nodes); // Ensure nodes are not null
            Guard.IsNotEmpty(nodes); // Ensure nodes are not empty
            var versionToken = nodes[0]["version"]; // Get version
            Guard.IsNotNull(versionToken); // Ensure version is not null
            var versionString = versionToken.Value<string>(); // Convert version to string
            Guard.IsNotNullOrWhiteSpace(versionString); // Ensure version is not empty
            Guard.IsTrue(Version.TryParse(versionString, out var lastestVersion)); // Parse latest version
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