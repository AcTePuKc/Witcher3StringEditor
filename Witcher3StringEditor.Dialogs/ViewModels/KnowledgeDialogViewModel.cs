using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using iNKORE.UI.WPF.Modern.Controls;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class KnowledgeDialogViewModel(IKnowledgeService knowledgeService)
    : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;
    
    public ObservableCollection<IW3KItem> KnowledgeItems { get; } = [];

    [RelayCommand]
    private async Task Search(AutoSuggestBoxQuerySubmittedEventArgs eventArgs)
    {
        var text = eventArgs.QueryText;
        if (string.IsNullOrWhiteSpace(text)) return;
        var results = knowledgeService.Search(text, 10);
        if (results == null) return;
        if (KnowledgeItems.Any()) KnowledgeItems.Clear();
        await foreach (var item in results) KnowledgeItems.Add(item);
    }

    [RelayCommand]
    private async Task LearnAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        await knowledgeService.Learn(text);
    }

    [RelayCommand]
    private async Task DeleteAsync(object[]? items)
    {
        if (items == null || items.Length == 0) return;
        await knowledgeService.Delete(items.Cast<IW3KItem>().Select(x => x.Id));
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await knowledgeService.Clear();
    }
}