namespace Witcher3StringEditor.Shared.Abstractions;

public interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}