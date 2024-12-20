using Newtonsoft.Json;
using System.IO;

namespace Witcher3StringEditor.Core;

public sealed class SettingsManager
{
    public static T Load<T>() where T : new()
    {
        if (File.Exists("Config.json"))
        {
            var json = File.ReadAllText("Config.json");
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }
        return new T();
    }

    public static void Save<T>(T settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText("Config.json", json);
    }
}