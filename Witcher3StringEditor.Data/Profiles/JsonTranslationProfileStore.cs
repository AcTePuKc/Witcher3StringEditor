using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;
using Witcher3StringEditor.Data.Storage;

namespace Witcher3StringEditor.Data.Profiles;

public sealed class JsonTranslationProfileStore : ITranslationProfileStore
{
    private const string ProfilesFileName = "translation-profiles.json";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<TranslationProfile>> ListAsync(CancellationToken cancellationToken = default)
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

    public async Task<TranslationProfile?> GetAsync(string profileId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        var profiles = await ListAsync(cancellationToken);
        foreach (var profile in profiles)
        {
            if (string.Equals(profile.Id, profileId, StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }
        }

        return null;
    }

    private static string GetProfilesPath()
    {
        var directory = AppDataPathProvider.GetAppDataDirectory();
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, ProfilesFileName);
    }

    private sealed class TranslationProfileFile
    {
        public List<TranslationProfile> Profiles { get; set; } = [];
    }
}
