using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfilePreviewService : ITranslationProfilePreviewService
{
    public Task<string> BuildPreviewAsync(TranslationProfile? profile, CancellationToken cancellationToken = default)
    {
        // TODO: Build a readable profile summary once profile selection is wired into settings UI.
        return Task.FromResult(string.Empty);
    }
}
