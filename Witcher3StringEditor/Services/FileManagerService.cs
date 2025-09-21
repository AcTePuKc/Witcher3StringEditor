using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides file management functionality for W3 string items
///     Implements the IFileManagerService interface to handle deserializing files, setting output folders, and updating
///     recent items
/// </summary>
internal class FileManagerService(IAppSettings appSettings, IW3Serializer w3Serializer) : IFileManagerService
{
    /// <summary>
    ///     Deserializes W3 string items from the specified file
    /// </summary>
    /// <param name="fileName">The path to the file to deserialize</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized W3 string items</returns>
    public async Task<ObservableCollection<W3StringItemModel>> DeserializeW3StringItems(string fileName)
    {
        Log.Information("The file {FileName} is being opened...", fileName);
        var items = await w3Serializer.Deserialize(fileName);
        var orderedItems = items.OrderBy(x => x.StrId);
        return orderedItems.Select(x => new W3StringItemModel(x)).ToObservableCollection();
    }

    /// <summary>
    ///     Sets the output folder based on the specified file name
    /// </summary>
    /// <param name="fileName">The file name to extract the directory from</param>
    /// <param name="onOutputFolderChanged">The action to call when the output folder changes</param>
    public void SetOutputFolder(string fileName, Action<string> onOutputFolderChanged)
    {
        var folder = Path.GetDirectoryName(fileName);
        if (folder == null) return;
        onOutputFolderChanged(folder);
        Log.Information("Working directory set to {Folder}.", folder);
    }

    /// <summary>
    ///     Updates the recent items list with the specified file name
    ///     If the file is already in the list, updates its opened time; otherwise, adds it to the list
    /// </summary>
    /// <param name="fileName">The file name to add or update in the recent items list</param>
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