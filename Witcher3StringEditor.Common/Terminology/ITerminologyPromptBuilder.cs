using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Terminology;

public interface ITerminologyPromptBuilder
{
    Task<TerminologyPrompt> BuildAsync(
        TerminologyPack? terminologyPack,
        TerminologyPack? styleGuidePack,
        CancellationToken cancellationToken = default);
}
