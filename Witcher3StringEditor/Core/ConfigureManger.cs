using Newtonsoft.Json;
using System.IO;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Core
{
    public static class ConfigureManger
    {
        public static SettingsModel Load()
        {
            if (File.Exists("Config.json"))
            {
                var json = File.ReadAllText("Config.json");
                return JsonConvert.DeserializeObject<SettingsModel>(json) ?? new SettingsModel();
            }
            else
            {
                return new SettingsModel();
            }
        }

        public static void Save(SettingsModel settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            File.WriteAllText("Config.json", json);
        }
    }
}