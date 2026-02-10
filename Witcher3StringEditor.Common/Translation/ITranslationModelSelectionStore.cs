using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Translation;

/// <summary>
///     Persists provider/model selection state locally for settings UX.
/// </summary>
public interface ITranslationModelSelectionStore
{
    Task<TranslationModelSelection?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(TranslationModelSelection selection, CancellationToken cancellationToken = default);
}

/// <summary>
///     Local model selection state used by Settings placeholders.
/// </summary>
public sealed record TranslationModelSelection(
    string ProviderName,
    string? ModelName,
    string? BaseUrl);
