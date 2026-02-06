using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using GTranslate;

namespace Witcher3StringEditor.Services;

public interface ITranslationRouter
{
    Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record TranslationRouterRequest(
    string Text,
    ILanguage ToLanguage,
    ILanguage FromLanguage);
