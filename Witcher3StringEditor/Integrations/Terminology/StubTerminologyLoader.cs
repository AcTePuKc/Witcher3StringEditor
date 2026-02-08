using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Terminology;

public sealed class StubTerminologyLoader : ITerminologyLoader, IStyleGuideLoader
{
    public Task<TerminologyPack> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var name = string.IsNullOrWhiteSpace(path) ? "Stub Terminology" : Path.GetFileNameWithoutExtension(path);
        TerminologyPack pack = new(name, path, new List<TerminologyEntry>());
        return Task.FromResult(pack);
    }

    public Task<StyleGuide> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var name = string.IsNullOrWhiteSpace(path) ? "Stub Style Guide" : Path.GetFileNameWithoutExtension(path);
        StyleGuide guide = new(name, path, new List<string>(), new List<string>(), new List<string>());
        return Task.FromResult(guide);
    }
}
