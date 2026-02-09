using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

/// <summary>
///     Stub loader that returns empty results until profile import is implemented.
/// </summary>
public sealed class StubTranslationProfileLoader : ITranslationProfileLoader
{
    public Task<IReadOnlyList<TranslationProfile>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Profile path is required.", nameof(path));
        }

        // TODO: Parse profile JSON or other formats once profile import is approved.
        IReadOnlyList<TranslationProfile> profiles = Array.Empty<TranslationProfile>();
        return Task.FromResult(profiles);
    }
}
