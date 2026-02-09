using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

public sealed class JsonTranslationProfileStore : ITranslationProfileStore
{
    private const string ProfilesFileName = "translation-profiles.json";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<TranslationProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var filePath = GetProfilesPath();
        if (!File.Exists(filePath))
        {
            return Array.Empty<TranslationProfile>();
        }

        await using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<TranslationProfileFile>(
            stream,
            SerializerOptions,
            cancellationToken);

        return data?.Profiles ?? Array.Empty<TranslationProfile>();
    }

    public async Task SaveAsync(TranslationProfile profile, CancellationToken cancellationToken = default)
    {
        _ = profile ?? throw new ArgumentNullException(nameof(profile));

        var filePath = GetProfilesPath();
        var profiles = (await LoadProfilesAsync(filePath, cancellationToken)).ToList();
        var existingIndex = profiles.FindIndex(existing =>
            string.Equals(existing.Id, profile.Id, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            profiles[existingIndex] = profile;
        }
        else
        {
            profiles.Add(profile);
        }

        await SaveProfilesAsync(filePath, profiles, cancellationToken);
    }

    public async Task DeleteAsync(string profileId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return;
        }

        var filePath = GetProfilesPath();
        if (!File.Exists(filePath))
        {
            return;
        }

        var profiles = (await LoadProfilesAsync(filePath, cancellationToken)).ToList();
        profiles.RemoveAll(profile =>
            string.Equals(profile.Id, profileId, StringComparison.OrdinalIgnoreCase));

        await SaveProfilesAsync(filePath, profiles, cancellationToken);
    }

    private static async Task<IReadOnlyList<TranslationProfile>> LoadProfilesAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<TranslationProfile>();
        }

        await using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<TranslationProfileFile>(
            stream,
            SerializerOptions,
            cancellationToken);

        return data?.Profiles ?? Array.Empty<TranslationProfile>();
    }

    private static async Task SaveProfilesAsync(
        string filePath,
        IReadOnlyCollection<TranslationProfile> profiles,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        var data = new TranslationProfileFile
        {
            Profiles = profiles.ToList()
        };

        await JsonSerializer.SerializeAsync(stream, data, SerializerOptions, cancellationToken);
    }

    private static string GetProfilesPath()
    {
        var appDataBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if DEBUG
        var appFolder = "Witcher3StringEditor_Debug";
#else
        var appFolder = "Witcher3StringEditor";
#endif
        var directory = Path.Combine(appDataBase, appFolder);
        return Path.Combine(directory, ProfilesFileName);
    }

    private sealed class TranslationProfileFile
    {
        public List<TranslationProfile> Profiles { get; set; } = [];
    }
}
