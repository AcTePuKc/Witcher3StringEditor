using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides file management functionality for The Witcher 3 string items
///     Implements the IFileManagerService interface to handle deserializing files, setting output folders, and updating
///     recent items
/// </summary>
internal class FileManagerService(IAppSettings appSettings, IW3Serializer w3Serializer) : IFileManagerService
{
    /// <summary>
    ///     Deserializes The Witcher 3 string items from the specified file
    /// </summary>
    /// <param name="fileName">The path to the file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the deserialized The Witcher 3
    ///     string items
    /// </returns>
    public async Task<ObservableCollection<W3StringItemModel>> DeserializeW3StringItems(string fileName)
    {
        Log.Information("The file {FileName} is being opened...", fileName); // Log file opening
        var items = await w3Serializer.Deserialize(fileName); // Deserialize file contents
        var orderedItems = items.OrderBy(x => x.StrId); // Order items by ID
        return orderedItems.Select(x => new W3StringItemModel(x))
            .ToObservableCollection(); // Convert to observable collection
    }

    /// <summary>
    ///     Sets the output folder based on the specified file name
    /// </summary>
    /// <param name="fileName">The file name to extract the directory from</param>
    /// <param name="onOutputFolderChanged">The action to call when the output folder changes</param>
    public void SetOutputFolder(string fileName, Action<string> onOutputFolderChanged)
    {
        var folder = Path.GetDirectoryName(fileName); // Extract directory from file name
        if (folder is null) return; // Return if directory is null
        onOutputFolderChanged(folder); // Notify of folder change
        Log.Information("Working directory set to {Folder}.", folder); // Log folder change
    }

    /// <summary>
    ///     Updates the recent items list with the specified file name
    ///     If the file is already in the list, updates its opened time; otherwise, adds it to the list
    /// </summary>
    /// <param name="fileName">The file name to add or update in the recent items list</param>
    public void UpdateRecentItems(string fileName)
    {
        var foundItem = appSettings.RecentItems.FirstOrDefault(x => x.FilePath == fileName); // Find existing item
        if (foundItem is null) // If item not found
        {
            appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now)); // Add new recent item
            Log.Information("Added {FileName} to recent items.", fileName); // Log addition
        }
        else // If item found
        {
            foundItem.OpenedTime = DateTime.Now; // Update opened time
            Log.Information("The last opened time for file {FileName} has been updated.", fileName); // Log update
        }
    }
}