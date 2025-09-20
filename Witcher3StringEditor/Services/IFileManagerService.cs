using System.Collections.ObjectModel;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

public interface IFileManagerService
{
    Task<ObservableCollection<W3StringItemModel>> DeserializeW3StringItems(string fileName);

    void SetOutputFolder(string fileName, Action<string> onOutputFolderChanged);

    void UpdateRecentItems(string fileName);
}