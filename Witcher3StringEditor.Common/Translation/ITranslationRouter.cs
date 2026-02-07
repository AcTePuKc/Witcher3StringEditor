using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using GTranslate;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationRouter
{
    Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record TranslationRouterRequest(
    string Text,
    ILanguage ToLanguage,
    ILanguage FromLanguage,
    string? ProviderName = null,
    string? ModelName = null);
