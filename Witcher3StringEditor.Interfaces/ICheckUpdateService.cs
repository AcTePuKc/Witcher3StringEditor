namespace Witcher3StringEditor.Interfaces;

public interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}