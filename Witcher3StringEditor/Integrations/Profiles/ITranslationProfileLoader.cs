using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

/// <summary>
///     Loads translation profiles from a local file source. Stub-only for now.
/// </summary>
public interface ITranslationProfileLoader
{
    Task<IReadOnlyList<TranslationProfile>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default);
}
