using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateContentViewModel : ObservableObject, IAsyncDisposable
{
    private readonly ITranslator translator;
    private readonly IReadOnlyList<IEditW3Item> w3Items;
    private CancellationTokenSource? cancellationTokenSource;

    [ObservableProperty] private TranslateItemModel? currentTranslateItemModel;
    private bool disposedValue;

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

    public TranslateContentViewModel(IAppSettings appSettings, ITranslator translator, IEnumerable<IEditW3Item> w3Items,
        int index)
    {
        this.w3Items = [.. w3Items];
        this.translator = translator;
        Languages = translator.Name switch
        {
            "MicrosoftTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Microsoft)),
            "GoogleTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            "YandexTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            _ => Language.LanguageDictionary.Values
        };

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

    public async ValueTask DisposeAsync()
    {
        if (!disposedValue)
        {
            if (cancellationTokenSource != null)
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                    await cancellationTokenSource.CancelAsync();
                cancellationTokenSource.Dispose();
            }

            disposedValue = true;
            GC.SuppressFinalize(this);
            Log.Information("TranslateContentViewModel is being disposed.");
        }
    }

    partial void OnIndexOfItemsChanged(int value)
    {
        var item = w3Items[value];
        CurrentTranslateItemModel = new TranslateItemModel { Id = item.Id, Text = item.Text };
    }

    partial void OnIsBusyChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "TranslatorIsBusy");
    }

    [RelayCommand]
    private async Task Translate()
    {
        try
        {
            Guard.IsNotNull(CurrentTranslateItemModel);
            Guard.IsNotNullOrWhiteSpace(CurrentTranslateItemModel.Text);
            if (!string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
                && !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                    "TranslationNotEmpty")) return;
            IsBusy = true;
            CurrentTranslateItemModel.TranslatedText = string.Empty;
            Log.Information("Starting translation for item {Id} (from {FromLang} to {ToLang}).",
                CurrentTranslateItemModel.Id, FormLanguage, ToLanguage);
            cancellationTokenSource = new CancellationTokenSource();
            var translateTask = translator.TranslateAsync(CurrentTranslateItemModel.Text, ToLanguage, FormLanguage);
            var completedTask = await Task.WhenAny(
                translateTask,
                Task.Delay(Timeout.Infinite, cancellationTokenSource.Token)
            );
            if (completedTask is { IsCanceled: true })
            {
                _ = translateTask.ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        //ignored
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
                IsBusy = false;
                return;
            }

            var translation = (await translateTask).Translation;
            Guard.IsNotNullOrWhiteSpace(translation);
            CurrentTranslateItemModel.TranslatedText = translation;
            Log.Information("Translation completed for item {Id} (from {FromLang} to {ToLang}).",
                CurrentTranslateItemModel.Id, FormLanguage, ToLanguage);
            IsBusy = false;
        }
        catch (Exception ex)
        {
            _ = WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(ex.Message), "TranslateError");
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
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(string.Empty),
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

    private async Task Navigate(int indexChange)
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
            }

            IndexOfItems += indexChange;
            Log.Information("Translator {TranslatorName} moved to {Direction} item (new index: {NewIndex})",
                translator.Name, indexChange > 0 ? "next" : "previous", IndexOfItems);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to move to {Direction} item", indexChange > 0 ? "next" : "previous");
        }
    }

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private async Task Previous()
    {
        await Navigate(-1);
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private async Task Next()
    {
        await Navigate(1);
    }

    ~TranslateContentViewModel()
    {
        if (!disposedValue)
        {
            if (cancellationTokenSource == null) return;
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        Log.Information("TranslateContentViewModel is being finalized.");
    }
}