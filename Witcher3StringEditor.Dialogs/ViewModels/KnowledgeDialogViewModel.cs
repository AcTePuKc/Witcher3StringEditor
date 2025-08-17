using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

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
            foreach (var d in data.ConvertAll(x => JsonSerializer.Serialize(x, jsonSerializerOptions)))
                KnowledgeItems.Add(await knowledgeService.Learn(d));
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
    private async Task Delete(IEnumerable<object>? items)
    {
        var ids = items?.Cast<IW3KItem>().Select(x => x.Id).ToList();
        if (ids == null || ids.Count == 0) return;
        if (await knowledgeService.Delete(ids))
            foreach (var id in ids)
                _ = KnowledgeItems.Remove(KnowledgeItems.First(x => x.Id == id));
    }

    [RelayCommand]
    private async Task Clear()
    {
        await knowledgeService.Clear();
        KnowledgeItems.Clear();
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var w3KItems = knowledgeService.All();
        if (w3KItems == null) return;
        await foreach (var item in w3KItems) KnowledgeItems.Add(item);
    }

    [RelayCommand]
    public async Task Import()
    {
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter("Backup File", ".backup")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".backup")
        {
            await knowledgeService.Import(storageFile.LocalPath);
            KnowledgeItems.Clear();
            var w3KItems = knowledgeService.All();
            if (w3KItems == null) return;
            await foreach (var item in w3KItems) KnowledgeItems.Add(item);
        }
    }

    [RelayCommand]
    public async Task Export()
    {
        using var storageFile = await dialogService.ShowSaveFileDialogAsync(this, new SaveFileDialogSettings
        {
            Filters =
            [
                new FileFilter("Backup File", ".backup")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".backup")
        {
            await knowledgeService.Export(storageFile.LocalPath, KnowledgeItems);
        }
    }
}