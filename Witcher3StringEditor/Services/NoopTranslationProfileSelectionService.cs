using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Profiles;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationProfileSelectionService : ITranslationProfileSelectionService
{
    public Task<string?> GetSelectedProfileIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task SetSelectedProfileIdAsync(string? profileId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
