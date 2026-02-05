using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Terminology;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTerminologyPromptBuilder : ITerminologyPromptBuilder
{
    public Task<TerminologyPrompt> BuildAsync(
        TerminologyPack? terminologyPack,
        TerminologyPack? styleGuidePack,
        CancellationToken cancellationToken = default)
    {
        // TODO: Inject terminology/style guidance into provider prompts once routing is implemented.
        return Task.FromResult(new TerminologyPrompt());
    }
}
