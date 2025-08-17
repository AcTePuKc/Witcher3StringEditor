using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Syncfusion.XlsIO;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Interfaces;

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
            var data = worksheet.ExportData<W3KExcelData>(range.Row, range.Column, range.LastRow, range.LastColumn);
            foreach (var d in data.ConvertAll(x => JsonSerializer.Serialize(x)))
            {
                KnowledgeItems.Add(await knowledgeService.Learn(d));
            }
        }
    }

    [RelayCommand]
    private async Task Sync()
    {
        var w3KItems = knowledgeService.All();
        if (w3KItems == null) return;
        KnowledgeItems.Clear();
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