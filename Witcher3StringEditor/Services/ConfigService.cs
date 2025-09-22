using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides configuration file saving and loading functionality
///     Implements the IConfigService interface to handle serialization and deserialization of settings
/// </summary>
internal class ConfigService(string filePath) : IConfigService
{
    /// <summary>
    ///     Saves the specified settings to a configuration file
    /// </summary>
    /// <typeparam name="T">The type of settings to save</typeparam>
    /// <param name="settings">The settings to save</param>
    public void Save<T>(T settings)
    {
        File.WriteAllText(filePath, // Write to config file
            JsonConvert.SerializeObject(settings, Formatting.Indented,
                new StringEnumConverter())); // Serialize settings with indentation
    }

    /// <summary>
    ///     Loads settings from a configuration file
    /// </summary>
    /// <typeparam name="T">The type of settings to load</typeparam>
    /// <returns>The loaded settings, or a new instance if the file does not exist</returns>
    public T Load<T>() where T : new()
    {
        return File.Exists(filePath) // Check if config file exists
            ? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)) ??
              new T() // Deserialize or create new instance
            : new T(); // Create new instance if file doesn't exist
    }
}