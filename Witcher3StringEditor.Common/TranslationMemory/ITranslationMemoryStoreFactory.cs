using System;

namespace Witcher3StringEditor.Common.TranslationMemory;

/// <summary>
///     Creates translation memory store instances based on runtime settings.
/// </summary>
public interface ITranslationMemoryStoreFactory
{
    /// <summary>
    ///     Creates a translation memory store for the supplied settings.
    /// </summary>
    /// <param name="settings">Translation memory settings.</param>
    /// <returns>A translation memory store instance.</returns>
    ITranslationMemoryStore Create(TranslationMemorySettings settings);
}
