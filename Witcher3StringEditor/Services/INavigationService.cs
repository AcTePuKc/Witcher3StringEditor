namespace Witcher3StringEditor.Services;

public interface INavigationService
{
    public void NavigateToDirectory(string directoryPath);

    public void NavigateToUrl(string url);

    Task PlayGame();
}