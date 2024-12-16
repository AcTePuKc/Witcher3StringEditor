using System.IO;
using Newtonsoft.Json;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Core;

public static class ConfigureManger
{
    public static SettingsModel Load()
    {
        if (!File.Exists("Config.json")) return new SettingsModel();
        var json = File.ReadAllText("Config.json");
        return JsonConvert.DeserializeObject<SettingsModel>(json) ?? new SettingsModel();
    }

    public static void Save(SettingsModel settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText("Config.json", json);
    }
}