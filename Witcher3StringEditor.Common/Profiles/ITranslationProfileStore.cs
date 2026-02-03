using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfileStore
{
    Task<IReadOnlyList<TranslationProfile>> ListAsync(CancellationToken cancellationToken = default);

    Task<TranslationProfile?> GetAsync(string profileId, CancellationToken cancellationToken = default);
}
