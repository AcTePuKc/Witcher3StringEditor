using System.Collections.Generic;

namespace Witcher3StringEditor.Integrations.Providers;

public interface ITranslationProviderRegistry
{
    void Register(ITranslationProvider provider);

    bool TryGet(string name, out ITranslationProvider provider);

    IReadOnlyCollection<ITranslationProvider> GetAll();
}
