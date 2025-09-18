namespace Witcher3StringEditor.Services;

internal interface IConfigService
{
    public void Save<T>(T settings);

    public T Load<T>() where T : new();
}