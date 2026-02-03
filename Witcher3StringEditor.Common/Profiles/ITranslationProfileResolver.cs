using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfileResolver
{
    Task<TranslationProfile?> ResolveAsync(string? profileId, CancellationToken cancellationToken = default);
}
