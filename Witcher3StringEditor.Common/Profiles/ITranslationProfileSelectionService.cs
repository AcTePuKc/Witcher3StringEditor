using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public interface ITranslationProfileSelectionService
{
    Task<string?> GetSelectedProfileIdAsync(CancellationToken cancellationToken = default);

    Task SetSelectedProfileIdAsync(string? profileId, CancellationToken cancellationToken = default);
}
