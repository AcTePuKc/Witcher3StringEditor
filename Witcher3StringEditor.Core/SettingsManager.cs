using Newtonsoft.Json;

namespace Witcher3StringEditor.Core;

public class SettingsManager
{
    private readonly string path;

    private static readonly Lazy<SettingsManager> LazyInstance
        = new(static () => new SettingsManager("Config.json"));

    private SettingsManager(string path) => this.path = path;

    public static SettingsManager Instance => LazyInstance.Value;

    public T Load<T>() where T : new()
    {
        if (!File.Exists(path)) return new T();
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json) ?? new T();
    }

    public void Save<T>(T settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}