using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationPipelineContextBuilder
{
    Task<TranslationPipelineContext> BuildAsync(CancellationToken cancellationToken = default);
}
