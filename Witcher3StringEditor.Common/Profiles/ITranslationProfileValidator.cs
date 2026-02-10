using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Profiles;

public sealed record TranslationProfileValidationResult(bool IsValid, string? Message);

public interface ITranslationProfileValidator
{
    Task<TranslationProfileValidationResult> ValidateAsync(
        TranslationProfile? profile,
        CancellationToken cancellationToken = default);
}
