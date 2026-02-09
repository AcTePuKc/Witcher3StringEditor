using System;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationMemoryStoreFactory : ITranslationMemoryStoreFactory
{
    public ITranslationMemoryStore Create(TranslationMemorySettings settings)
    {
        _ = settings ?? throw new ArgumentNullException(nameof(settings));
        // TODO: Swap with a SQLite-backed store when translation memory wiring is approved.
        return new NoopTranslationMemoryStore();
    }
}
