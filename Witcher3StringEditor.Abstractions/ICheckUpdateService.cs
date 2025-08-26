namespace Witcher3StringEditor.Abstractions;

public interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}