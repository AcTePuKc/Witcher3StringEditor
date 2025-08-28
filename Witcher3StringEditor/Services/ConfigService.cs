using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Services;

internal class ConfigService(string path) : IConfigService
{
    public void Save<T>(T settings)
    {
        File.WriteAllText(path,
            JsonConvert.SerializeObject(settings, Formatting.Indented, new StringEnumConverter()));
    }
    
    public T Load<T>() where T : new()
    {
        return File.Exists(path)
            ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) ?? new T()
            : new T();
    }
}