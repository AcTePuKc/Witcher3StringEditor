using System;
using System.IO;

namespace Witcher3StringEditor.Data.Storage;

internal static class AppDataPathProvider
{
    private const string DatabaseFileName = "witcher3stringeditor.db";

    internal static string GetDatabasePath()
    {
        var appDataDirectory = GetAppDataDirectory();
        return Path.Combine(appDataDirectory, DatabaseFileName);
    }

    internal static string GetAppDataDirectory()
    {
        var appDataBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if DEBUG
        var appFolder = "Witcher3StringEditor_Debug";
#else
        var appFolder = "Witcher3StringEditor";
#endif
        return Path.Combine(appDataBase, appFolder);
    }
}
