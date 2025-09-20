using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

internal class NavigationService(
    IAppSettings appSettings,
    IExplorerService explorerService,
    IPlayGameService playGameService):INavigationService
{
    public void NavigateToDirectory(string directoryPath)
    {
        explorerService.Open(directoryPath);
        Log.Information("Navigated to directory: {Directory}", directoryPath);    
    }

    public void NavigateToUrl(string url)
    {
        explorerService.Open(url);
        Log.Information("Navigated to URL: {Url}", url);
    }

    public async Task PlayGame()
    {
        await playGameService.PlayGame();
    }
}