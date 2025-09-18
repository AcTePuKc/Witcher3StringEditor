namespace Witcher3StringEditor.Services;

internal interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}