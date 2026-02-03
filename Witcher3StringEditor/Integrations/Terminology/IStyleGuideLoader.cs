using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Terminology;

public interface IStyleGuideLoader
{
    Task<StyleGuide> LoadAsync(string path, CancellationToken cancellationToken = default);
}
