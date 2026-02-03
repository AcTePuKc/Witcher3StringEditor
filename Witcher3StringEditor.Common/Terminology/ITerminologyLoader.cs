using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Terminology;

public interface ITerminologyLoader
{
    Task<TerminologyPack> LoadAsync(string path, CancellationToken cancellationToken = default);
}
