using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Terminology;

public interface ITerminologyValidationService
{
    Task<TerminologyValidationResult> ValidateTerminologyAsync(string path,
        CancellationToken cancellationToken = default);

    Task<TerminologyValidationResult> ValidateStyleGuideAsync(string path,
        CancellationToken cancellationToken = default);
}
