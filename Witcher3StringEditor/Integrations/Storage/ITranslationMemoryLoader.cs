using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Storage;

/// <summary>
///     Loads translation memory entries from a local file source. Stub-only for now.
/// </summary>
public interface ITranslationMemoryLoader
{
    Task<IReadOnlyList<TranslationMemoryEntry>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default);
}
