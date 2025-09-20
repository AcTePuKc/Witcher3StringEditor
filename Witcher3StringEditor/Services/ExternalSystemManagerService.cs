using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

internal class ExternalSystemManagerService(
    IAppSettings appSettings,
    IExplorerService explorerService,
    IPlayGameService playGameService):IExternalSystemManagerService
{
    public void OpenWorkingFolder(string outputFolder)
    {
        explorerService.Open(outputFolder);
        Log.Information("Working folder opened.");
    }

    public void OpenNexusMods()
    {
        explorerService.Open(appSettings.NexusModUrl);
        Log.Information("NexusMods opened.");
    }

    public async Task PlayGame()
    {
        await playGameService.PlayGame();
    }
}