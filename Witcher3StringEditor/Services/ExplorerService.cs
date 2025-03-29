using System.Diagnostics;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class ExplorerService : IExplorerService
{
    public void Open(string path)
    {
        using var _ = Process.Start("explorer.exe", path);
    }
}