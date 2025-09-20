using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Services;

internal class FileManagerService(IAppSettings appSettings, IW3Serializer w3Serializer) : IFileManagerService
{
    public async Task<ObservableCollection<W3StringItemModel>> DeserializeW3StringItems(string fileName)
    {
        Log.Information("The file {FileName} is being opened...", fileName);
        var items = await w3Serializer.Deserialize(fileName);
        var orderedItems = items.OrderBy(x => x.StrId);
        return orderedItems.Select(x => new W3StringItemModel(x)).ToObservableCollection();
    }

    public void SetOutputFolder(string fileName, Action<string> onOutputFolderChanged)
    {
        var folder = Path.GetDirectoryName(fileName);
        if (folder == null) return;
        onOutputFolderChanged(folder);
        Log.Information("Working directory set to {Folder}.", folder);
    }

    public void UpdateRecentItems(string fileName)
    {
        var foundItem = appSettings.RecentItems.FirstOrDefault(x => x.FilePath == fileName);
        if (foundItem == null)
        {
            appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
            Log.Information("Added {FileName} to recent items.", fileName);
        }
        else
        {
            foundItem.OpenedTime = DateTime.Now;
            Log.Information("The last opened time for file {FileName} has been updated.", fileName);
        }
    }
}