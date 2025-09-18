namespace Witcher3StringEditor.Services;

public interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}