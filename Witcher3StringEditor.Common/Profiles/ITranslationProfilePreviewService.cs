using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfilePreviewService
{
    Task<string> BuildPreviewAsync(TranslationProfile? profile, CancellationToken cancellationToken = default);
}
