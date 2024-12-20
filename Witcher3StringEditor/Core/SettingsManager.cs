using Newtonsoft.Json;
using System.IO;

namespace Witcher3StringEditor.Core;

public class SettingsManager(string path)
{
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