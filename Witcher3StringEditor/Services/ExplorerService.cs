using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static")]
internal class ExplorerService : IExplorerService
{
    [SuppressMessage("Performance", "CA1822:将成员标记为 static")]
    public void Open(string path)
    {
        using var _ = Process.Start("explorer.exe", path);
    }
}