using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    private readonly IEnumerable<W3Item> w3Items;
    private readonly MicrosoftTranslator translator = new();

    public ObservableCollection<Language> Languages { get; }
        = new(Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft)));

    [ObservableProperty]
    private Language toLanguage;

    [ObservableProperty]
    private TranslateItem? currentTranslateItemModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int indexOfItems = -1;

    partial void OnIndexOfItemsChanged(int value)
    {
        var item = w3Items.ElementAt(value);
        CurrentTranslateItemModel = new TranslateItem { Id = item.Id, Text = item.Text };
    }

    public TranslateDiaglogViewModel(IEnumerable<W3Item> w3Items, int index,IAppSettings appSettings)
    {
        this.w3Items = w3Items;
        IndexOfItems = index;
        var language = appSettings.PreferredLanguage;
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
        if (CurrentTranslateItemModel == null) return;
        var result = await translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage);
        CurrentTranslateItemModel.TranslatedText = result.Translation;
    }

    [RelayCommand]
    private void Save()
    {
        if (CurrentTranslateItemModel == null) return;
        w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
    }

    private bool CanPrevious() => IndexOfItems > 0;

    private bool CanNext() => IndexOfItems < w3Items.Count() - 1;

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