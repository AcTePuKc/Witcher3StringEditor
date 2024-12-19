using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public partial class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public bool? DialogResult => true;

        public event EventHandler? RequestClose;

        private readonly MicrosoftTranslator translator = new MicrosoftTranslator();

        private readonly IEnumerable<W3ItemModel> w3ItemModels;

        public ObservableCollection<Language> Languages { get; set; } = new(Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft)));

        [ObservableProperty]
        private Language toLanguage = Language.GetLanguage("zh-CN");

        [ObservableProperty]
        private TranslateItemModel currentTranslateItemModel;

        public TranslateDiaglogViewModel(IEnumerable<W3ItemModel> w3Items)
        {
            w3ItemModels = w3Items;
            var itemModel = w3ItemModels.First();
            CurrentTranslateItemModel = new TranslateItemModel() { Id = itemModel.Id, Text = itemModel.Text };
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
            w3ItemModels.Where(x => x.Id == CurrentTranslateItemModel.Id).First().Text = CurrentTranslateItemModel.TranslatedText;
        }
    }
}