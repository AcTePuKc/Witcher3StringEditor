using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Profiles;

public interface ITranslationProfileStore
{
    Task<IReadOnlyList<TranslationProfile>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(TranslationProfile profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(string profileId, CancellationToken cancellationToken = default);
}
