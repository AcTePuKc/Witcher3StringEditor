using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationRouter : ITranslationRouter
{
    public Task<Result<string>> TranslateAsync(
        TranslationRouterRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Fail<string>("Translation router is not implemented."));
    }
}
