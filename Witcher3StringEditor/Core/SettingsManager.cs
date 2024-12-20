using Newtonsoft.Json;
using System.IO;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Core;

public sealed class SettingsManager
{
    public static Settings LoadConfiguration()
    {
        if (File.Exists("Config.json"))
        {
            var json = File.ReadAllText("Config.json");
            return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
        }
        return new Settings();
    }

    public static void SaveConfiguration(Settings settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText("Config.json", json);
    }
}