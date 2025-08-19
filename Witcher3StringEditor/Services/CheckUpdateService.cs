using CommunityToolkit.Diagnostics;
using Serilog;
using System.Net.Http;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class CheckUpdateService() : ICheckUpdateService
{
    private static string UpdateUrl => "https://witcher3stringeditorcheckupdate.azurewebsites.net/api/checkupdate";

    public async Task<bool> CheckUpdate()
    {
        try
        {
            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(UpdateUrl);
            if (!httpResponse.IsSuccessStatusCode) return true;
            Guard.IsTrue(Version.TryParse(await httpResponse.Content.ReadAsStringAsync(), out var lastestVersion));
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