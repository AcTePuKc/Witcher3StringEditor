using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Terminology;

public interface IStyleGuideLoader
{
    Task<StyleGuide> LoadStyleGuideAsync(string path, CancellationToken cancellationToken = default);
}
