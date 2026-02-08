using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationPostProcessor
{
    string Name { get; }

    Task<TranslationPostProcessingResult> ProcessAsync(
        TranslationPostProcessingRequest request,
        CancellationToken cancellationToken = default);
}
