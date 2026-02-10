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
    string? ModelName = null,
    string? BaseUrl = null,
    bool UseProviderForTranslation = false,
    TranslationPipelineContext? PipelineContext = null)
{
    public string? Source { get; init; }
    public string? Target { get; init; }
    public string? Provider { get; init; }
    public string? Model { get; init; }
}

public sealed record TranslationRouterResult(
    string Status,
    string? Error = null);
