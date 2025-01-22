using System.Diagnostics;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class ExplorerService : IExplorerService
{
    public void Open(string path)
        => Process.Start("explorer.exe", path);
}