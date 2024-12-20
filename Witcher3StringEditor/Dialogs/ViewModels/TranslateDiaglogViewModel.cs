using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    private readonly IEnumerable<W3Item> w3ItemModels;
    private readonly SettingsManager settingsManager = new("Config.json");
    private readonly MicrosoftTranslator translator = new();

    public ObservableCollection<Language> Languages { get; }
        = new(Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft)));

    [ObservableProperty]
    private Language toLanguage;

    [ObservableProperty]
    private TranslateItem currentTranslateItemModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int indexOfItems;

    partial void OnIndexOfItemsChanged(int value)
    {
        var itemModel = w3ItemModels.ElementAt(value);
        CurrentTranslateItemModel = new TranslateItem { Id = itemModel.Id, Text = itemModel.Text };
    }

    public TranslateDiaglogViewModel(IEnumerable<W3Item> w3Items, int index)
    {
        IndexOfItems = index;
        w3ItemModels = w3Items;
        var itemModel = w3ItemModels.ElementAt(IndexOfItems);
        CurrentTranslateItemModel = new TranslateItem { Id = itemModel.Id, Text = itemModel.Text };
        var language = settingsManager.Load<Settings>().PreferredLanguage;
        ToLanguage = language switch
        {
            W3Language.br => Language.GetLanguage("pt"),
            W3Language.cn => Language.GetLanguage("zh-CN"),
            W3Language.esmx => Language.GetLanguage("es"),
            W3Language.cz => Language.GetLanguage("cs"),
            W3Language.jp => Language.GetLanguage("ja"),
            W3Language.kr => Language.GetLanguage("ko"),
            W3Language.zh => Language.GetLanguage("zh-TW"),
            _ => Language.GetLanguage(Enum.GetName(language) ?? "en")
        };
    }

    [RelayCommand]
    private async Task Translate()
    {
        var result = await translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage);
        CurrentTranslateItemModel.TranslatedText = result.Translation;
    }

    [RelayCommand]
    private void Save()
    {
        w3ItemModels.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
    }

    private bool CanPrevious() => IndexOfItems > 0;

    private bool CanNext() => IndexOfItems < w3ItemModels.Count() - 1;

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private void Previous()
    {
        IndexOfItems -= 1;
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        IndexOfItems += 1;
    }
}