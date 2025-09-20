namespace Witcher3StringEditor.Services;

public interface IExternalSystemManagerService
{
    void OpenWorkingFolder(string outputFolder);

    void OpenNexusMods();

    Task PlayGame();
}