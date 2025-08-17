using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Interfaces;
using Syncfusion.XlsIO;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class KnowledgeDialogViewModel(IKnowledgeService knowledgeService, IDialogService dialogService)
    : ObservableObject, IModalDialogViewModel
{
    public ObservableCollection<IW3KItem> KnowledgeItems { get; } = [];
    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Learn()
    {
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatExcelWorkSheets, ".xlsx")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".xlsx")
        {
            using var excelEngine = new ExcelEngine();
            var worksheet = excelEngine.Excel.Workbooks.OpenReadOnly(storageFile.LocalPath).Worksheets[0];
            var range = worksheet.UsedRange;
            
        }
    }

    [RelayCommand]
    private async Task Sync()
    {
        var w3KItems = knowledgeService.All();
        if (w3KItems == null) return;
        await foreach (var item in w3KItems) KnowledgeItems.Add(item);
    }

    [RelayCommand]
    private async Task DeleteAsync(IEnumerable<object>? items)
    {
        if (items is not object[] enumerable || enumerable.Length == 0) return;
        await knowledgeService.Delete(enumerable.Cast<IW3KItem>().Select(x => x.Id));
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await knowledgeService.Clear();
    }
}