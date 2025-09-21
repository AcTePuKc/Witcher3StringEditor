using System.Collections.ObjectModel;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Defines a contract for file management operations related to The Witcher 3 string items
///     Provides methods to deserialize The Witcher 3 string items, set output folders, and update recent items
/// </summary>
internal interface IFileManagerService
{
    /// <summary>
    ///     Deserializes The Witcher 3 string items from the specified file
    /// </summary>
    /// <param name="fileName">The path to the file to deserialize</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized The Witcher 3 string items</returns>
    Task<ObservableCollection<W3StringItemModel>> DeserializeW3StringItems(string fileName);

    /// <summary>
    ///     Sets the output folder based on the specified file name
    /// </summary>
    /// <param name="fileName">The file name to extract the directory from</param>
    /// <param name="onOutputFolderChanged">The action to call when the output folder changes</param>
    void SetOutputFolder(string fileName, Action<string> onOutputFolderChanged);

    /// <summary>
    ///     Updates the recent items list with the specified file name
    ///     If the file is already in the list, updates its opened time; otherwise, adds it to the list
    /// </summary>
    /// <param name="fileName">The file name to add or update in the recent items list</param>
    void UpdateRecentItems(string fileName);
}