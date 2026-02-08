using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfileSettingsResolver
{
    Task<TranslationProfile?> ResolveAsync(
        IAppSettings settings,
        CancellationToken cancellationToken = default);
}
