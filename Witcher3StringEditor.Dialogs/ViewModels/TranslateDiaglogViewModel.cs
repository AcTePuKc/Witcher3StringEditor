using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    private readonly IEnumerable<IW3Item> w3Items;
    private readonly ITranslator translator = new MicrosoftTranslator();

    public IEnumerable<ILanguage> Languages { get; }
        = Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft));

    [ObservableProperty]
    private ILanguage toLanguage;

    [ObservableProperty]
    private ILanguage formLanguage;

    [ObservableProperty]
    private TranslateItem? currentTranslateItemModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int indexOfItems = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isTransLating;

    partial void OnIndexOfItemsChanged(int value)
    {
        var item = w3Items.ElementAt(value);
        CurrentTranslateItemModel = new TranslateItem { Id = item.Id, Text = item.Text };
    }

    public TranslateDiaglogViewModel(IEnumerable<IW3Item> w3Items, int index, IAppSettings appSettings)
    {
        this.w3Items = w3Items;
        IndexOfItems = index;
        FormLanguage = Language.GetLanguage("en");
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
    private async Task Closing()
    {
        if (CurrentTranslateItemModel is { IsSaved: false }
            && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new TranslatedTextNoSavedMessage()))
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
    }

    [RelayCommand]
    private async Task Translate()
    {
        if (CurrentTranslateItemModel == null) return;
        if (CurrentTranslateItemModel.Text.Length <= 1000)
        {
            try
            {
                IsTransLating = true;
                var result = await translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage, FormLanguage);
                CurrentTranslateItemModel.TranslatedText = result.Translation;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new SimpleStringMessage(ex.Message), "TranslateError");
                Log.Error(ex.Message);
            }
            IsTransLating = false;
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new SimpleStringMessage(Strings.TranslateCharactersNumberExceedLimitMessage), "TranslateCharactersNumberExceedLimit");
        }
    }

    private bool CanSave => !IsTransLating;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (CurrentTranslateItemModel == null) return;
        if (string.IsNullOrEmpty(CurrentTranslateItemModel.TranslatedText))
            WeakReferenceMessenger.Default.Send(new SimpleStringMessage(Strings.TranslatedTextInvalidMessage), "TranslatedTextInvalid");
        else
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        CurrentTranslateItemModel.IsSaved = true;
    }

    private bool CanPrevious => IndexOfItems > 0 && !IsTransLating;

    private bool CanNext => IndexOfItems < w3Items.Count() - 1 && !IsTransLating;

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private async Task PreviousAsync()
    {
        if (CurrentTranslateItemModel is { IsSaved: false }
            && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new TranslatedTextNoSavedMessage()))
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        IndexOfItems -= 1;
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private async Task Next()
    {
        if (CurrentTranslateItemModel is { IsSaved: false }
            && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new TranslatedTextNoSavedMessage()))
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        IndexOfItems += 1;
    }
}