using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateContentViewModel : ObservableObject
{
    private readonly ITranslator translator;
    private readonly IEnumerable<IW3Item> w3Items;

    [ObservableProperty] private TranslateItem? currentTranslateItemModel;

    [ObservableProperty] private ILanguage formLanguage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int indexOfItems = -1;

    [ObservableProperty] private bool isAiTranslator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    [ObservableProperty] private IEnumerable<ILanguage> languages;

    [ObservableProperty] private ILanguage toLanguage;

    public TranslateContentViewModel(IEnumerable<IW3Item> w3Items, int index, IAppSettings appSettings,
        ITranslator translator)
    {
        this.w3Items = w3Items;
        this.translator = translator;
        IsAiTranslator = translator is not MicrosoftTranslator;
        Languages = IsAiTranslator
            ? Language.LanguageDictionary.Values
            : Language.LanguageDictionary.Values
                .Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft));
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

    private bool CanSave => !IsBusy;

    private bool CanPrevious => IndexOfItems > 0 && !IsBusy;

    private bool CanNext => IndexOfItems < w3Items.Count() - 1 && !IsBusy;

    partial void OnIndexOfItemsChanged(int value)
    {
        var item = w3Items.ElementAt(value);
        CurrentTranslateItemModel = new TranslateItem { Id = item.Id, Text = item.Text };
    }

    partial void OnIsBusyChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage<bool>(value), "TranslatorIsBusy");
    }

    [RelayCommand]
    private async Task Translate()
    {
        if (CurrentTranslateItemModel == null) return;
        if (CurrentTranslateItemModel.Text.Length <= 1000 || IsAiTranslator)
        {
            try
            {
                Guard.IsNotNullOrWhiteSpace(CurrentTranslateItemModel.Text);
                IsBusy = true;
                CurrentTranslateItemModel.TranslatedText = string.Empty;
                CurrentTranslateItemModel.TranslatedText =
                    (await translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage, FormLanguage))
                    .Translation;
            }
            catch (Exception ex)
            {
                _ = WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(ex.Message), "TranslateError");
                Log.Error(ex, "Translation error occurred.}");
            }

            IsBusy = false;
        }
        else
        {
            _ = WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(string.Empty),
                "TranslateCharactersNumberExceedLimit");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (CurrentTranslateItemModel == null) return;
        if (string.IsNullOrEmpty(CurrentTranslateItemModel.TranslatedText))
            WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(string.Empty), "TranslatedTextInvalid");
        else
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        CurrentTranslateItemModel.IsSaved = true;
    }

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private async Task Previous()
    {
        if (CurrentTranslateItemModel is { IsSaved: false }
            && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        IndexOfItems -= 1;
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private async Task Next()
    {
        if (CurrentTranslateItemModel is { IsSaved: false }
            && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
            w3Items.First(x => x.Id == CurrentTranslateItemModel.Id).Text = CurrentTranslateItemModel.TranslatedText;
        IndexOfItems += 1;
    }
}