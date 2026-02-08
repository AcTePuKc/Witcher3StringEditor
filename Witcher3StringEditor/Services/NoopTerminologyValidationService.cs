using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Terminology;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTerminologyValidationService : ITerminologyValidationService
{
    public Task<TerminologyValidationResult> ValidateTerminologyAsync(string path,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TerminologyValidationResult.NotValidated);
    }

    public Task<TerminologyValidationResult> ValidateStyleGuideAsync(string path,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TerminologyValidationResult.NotValidated);
    }
}
