using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.TranslationMemory;

public interface ITranslationMemoryDatabaseInitializer
{
    Task InitializeAsync(TranslationMemorySettings settings, CancellationToken cancellationToken = default);
}
