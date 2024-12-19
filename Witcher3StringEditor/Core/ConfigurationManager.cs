using Newtonsoft.Json;
using System.IO;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Core;

public sealed class ConfigurationManager
{
    public static SettingsModel LoadConfiguration()
    {
        if (File.Exists("Config.json"))
        {
            var json = File.ReadAllText("Config.json");
            return JsonConvert.DeserializeObject<SettingsModel>(json) ?? new SettingsModel();
        }
        return new SettingsModel();
    }

    public static void SaveConfiguration(SettingsModel settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText("Config.json", json);
    }
}