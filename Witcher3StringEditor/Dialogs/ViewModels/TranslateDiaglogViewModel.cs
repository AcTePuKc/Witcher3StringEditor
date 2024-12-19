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
    public partial class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;

        private readonly W3ItemModel[] w3ItemModels;

        private readonly MicrosoftTranslator translator = new();

        public ObservableCollection<Language> Languages { get; set; } 
            = new(Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft)));

        [ObservableProperty]
        private Language toLanguage = Language.GetLanguage("zh-CN");

        [ObservableProperty]
        private TranslateItemModel currentTranslateItemModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
        [NotifyCanExecuteChangedFor(nameof(NextCommand))]
        private int indexOfItems = 0;

        partial void OnIndexOfItemsChanged(int value)
        {
            var itemModel = w3ItemModels[value];
            CurrentTranslateItemModel = new TranslateItemModel() { Id = itemModel.Id, Text = itemModel.Text };
        }

        public TranslateDiaglogViewModel(IEnumerable<W3ItemModel> w3Items)
        {
            w3ItemModels = w3Items.ToArray();
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

        private bool CanPrevious() => IndexOfItems > 0;

        private bool CanNext() => IndexOfItems < w3ItemModels.Length - 1;

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
}