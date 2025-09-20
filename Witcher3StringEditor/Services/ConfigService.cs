using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Witcher3StringEditor.Services;

internal class ConfigService(string filePath) : IConfigService
{
    public void Save<T>(T settings)
    {
        File.WriteAllText(filePath,
            JsonConvert.SerializeObject(settings, Formatting.Indented, new StringEnumConverter()));
    }

    public T Load<T>() where T : new()
    {
        return File.Exists(filePath)
            ? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)) ?? new T()
            : new T();
    }
}