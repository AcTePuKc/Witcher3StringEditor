using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Data.TranslationMemory;

public sealed class NoopTranslationMemoryDatabaseInitializer : ITranslationMemoryDatabaseInitializer
{
    public Task InitializeAsync(TranslationMemorySettings settings, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
