namespace Witcher3StringEditor.Interfaces;

public interface ICheckUpdateService
{
    public ValueTask<bool> CheckUpdate();
}