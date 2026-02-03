using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Terminology;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTerminologyLoader : ITerminologyLoader
{
    public Task<TerminologyPack> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with CSV/TSV/Markdown loaders.
        return Task.FromResult(new TerminologyPack
        {
            Name = "(stub)",
            Entries = []
        });
    }
}
