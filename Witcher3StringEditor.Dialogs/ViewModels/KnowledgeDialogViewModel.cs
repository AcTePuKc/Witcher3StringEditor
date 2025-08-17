using System.Collections.ObjectModel;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using iNKORE.UI.WPF.Modern.Controls;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class KnowledgeDialogViewModel(IKnowledgeService knowledgeService) : ObservableObject, IModalDialogViewModel
{
    private readonly IKnowledgeService knowledgeService = knowledgeService;

    public bool? DialogResult => true;

    public ObservableCollection<IW3KItem> KnowledgeItems { get; } = [];

    [RelayCommand]
    private async Task Sync()
    {
        var w3KItems = knowledgeService.All();
        if (w3KItems == null) return;
        await foreach (var item in w3KItems)
        {
            KnowledgeItems.Add(item);
        }
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