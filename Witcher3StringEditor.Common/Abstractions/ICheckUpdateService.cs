namespace Witcher3StringEditor.Common.Abstractions;

public interface ICheckUpdateService
{
    public Task<bool> CheckUpdate();
}