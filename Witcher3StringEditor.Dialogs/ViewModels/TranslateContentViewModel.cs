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
    private readonly IReadOnlyList<IW3Item> w3Items;

    [ObservableProperty] private TranslateItem? currentTranslateItemModel;

    [ObservableProperty] private ILanguage formLanguage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int indexOfItems = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    [ObservableProperty] private IEnumerable<ILanguage> languages;

    [ObservableProperty] private ILanguage toLanguage;

    public TranslateContentViewModel(IAppSettings appSettings, ITranslator translator, IEnumerable<IW3Item> w3Items,
        int index)
    {
        this.w3Items = [.. w3Items];
        this.translator = translator;
        Languages = Language.LanguageDictionary.Values
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

    private bool CanNext => IndexOfItems < w3Items.Count - 1 && !IsBusy;

    partial void OnIndexOfItemsChanged(int oldValue, int newValue)
    {
        var item = w3Items[newValue];
        CurrentTranslateItemModel = new TranslateItem { Id = item.Id, Text = item.Text };
        Log.Information("Translated item index changed from {OldIndex} to {NewIndex} (Item ID: {ItemId})", oldValue,
            newValue, item.Id);
    }

    partial void OnIsBusyChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage<bool>(value), "TranslatorIsBusy");
    }

    [RelayCommand]
    private async Task Translate()
    {
        try
        {
            Guard.IsNotNull(CurrentTranslateItemModel);
            if (CurrentTranslateItemModel.Text.Length > 1000)
            {
                _ = WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(string.Empty),
                    "TranslateCharactersNumberExceedLimit");
                Log.Error("Exceeded the character limit for translator {0}.", translator.Name);
            }
            else
            {
                Guard.IsNotNullOrWhiteSpace(CurrentTranslateItemModel.Text);
                if (!string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
                    && !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                        "TranslationNotEmpty")) return;
                IsBusy = true;
                CurrentTranslateItemModel.TranslatedText = string.Empty;
                Log.Information("Starting translation for item {Id} (from {FromLang} to {ToLang}).",
                    CurrentTranslateItemModel.Id, FormLanguage, ToLanguage);
                var translation =
                    (await translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage, FormLanguage))
                    .Translation;
                Guard.IsNotNullOrWhiteSpace(translation);
                CurrentTranslateItemModel.TranslatedText = translation;
                Log.Information("Translation completed for item {Id} (from {FromLang} to {ToLang}).",
                    CurrentTranslateItemModel.Id, FormLanguage, ToLanguage);
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            _ = WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(ex.Message), "TranslateError");
            Log.Error(ex, "Translation failed for item {ItemId} (From: {FromLang} To: {ToLang}).",
                CurrentTranslateItemModel?.Id, FormLanguage, ToLanguage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            Guard.IsNotNull(CurrentTranslateItemModel);
            if (!string.IsNullOrEmpty(CurrentTranslateItemModel.TranslatedText))
            {
                var found = w3Items.First(x => x.Id == CurrentTranslateItemModel.Id);
                Guard.IsNotNull(found);
                found.Text = CurrentTranslateItemModel.TranslatedText;
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(string.Empty),
                    "TranslatedTextInvalid");
            }

            CurrentTranslateItemModel.IsSaved = true;
            Log.Information("Translation saved for item {Id}.", CurrentTranslateItemModel.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save translation for item {ItemId}.", CurrentTranslateItemModel?.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private async Task Previous()
    {
        try
        {
            if (CurrentTranslateItemModel is { IsSaved: false } &&
                !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText) &&
                await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
            {
                var found = w3Items.First(x => x.Id == CurrentTranslateItemModel.Id);
                Guard.IsNotNull(found);
                found.Text = CurrentTranslateItemModel.TranslatedText;
                IndexOfItems -= 1;
                Log.Information("Translator {0} moved to the previous item.", translator.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to move to the previous item.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private async Task Next()
    {
        try
        {
            if (CurrentTranslateItemModel is { IsSaved: false }
                && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
                && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
            {
                var found = w3Items.First(x => x.Id == CurrentTranslateItemModel.Id);
                Guard.IsNotNull(found);
                found.Text = CurrentTranslateItemModel.TranslatedText;
                IndexOfItems += 1;
                Log.Information("Translator {0} moved to the next item.", translator.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to move to the next item.");
        }
    }
}