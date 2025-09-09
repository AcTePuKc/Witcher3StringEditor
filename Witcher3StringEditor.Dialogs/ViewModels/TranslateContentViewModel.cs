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
    private readonly ITranslator _translator;
    private readonly IReadOnlyList<IEditW3Item> _w3Items;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty] private TranslateItemModel? _currentTranslateItemModel;
    private bool _disposedValue;

    [ObservableProperty] private ILanguage _formLanguage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int _indexOfItems = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty] private IEnumerable<ILanguage> _languages;

    [ObservableProperty] private ILanguage _toLanguage;

    public TranslateContentViewModel(IAppSettings appSettings, ITranslator translator, IEnumerable<IEditW3Item> w3Items,
        int index)
    {
        _w3Items = [.. w3Items];
        _translator = translator;
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
        Log.Information("TranslateContentViewModel initialized.");
    }

    private bool CanSave => !IsBusy;

    private bool CanPrevious => IndexOfItems > 0 && !IsBusy;

    private bool CanNext => IndexOfItems < _w3Items.Count - 1 && !IsBusy;

    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource.Dispose();
            }

            _disposedValue = true;
            GC.SuppressFinalize(this);
            Log.Information("TranslateContentViewModel is being disposed.");
        }
    }

    partial void OnIndexOfItemsChanged(int value)
    {
        var item = _w3Items[value];
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
            _cancellationTokenSource = new CancellationTokenSource();
            Log.Information("Starting translation.");
            var (result, translation) = await ExecuteTranslationTask(_translator, CurrentTranslateItemModel.Text,
                ToLanguage,
                FormLanguage, _cancellationTokenSource);
            if (!result)
            {
                IsBusy = false;
                return;
            }

            Guard.IsNotNullOrWhiteSpace(translation);
            CurrentTranslateItemModel.TranslatedText = translation;
            Log.Information("Translation completed.");
            IsBusy = false;
        }
        catch (Exception ex)
        {
            _ = WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<string>(
                    $"The translator: {_translator.Name} returned an error.\nException: {ex.Message}"),
                "TranslateError");
            Log.Error(ex, "The translator: {Name} returned an error. Exception: {ExceptionMessage}", _translator.Name,
                ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<(bool, string)> ExecuteTranslationTask(ITranslator translator, string text,
        ILanguage toLanguage, ILanguage formLanguage, CancellationTokenSource cancellationTokenSource)
    {
        var translateTask = translator.TranslateAsync(text, toLanguage, formLanguage);
        var completedTask = await Task.WhenAny(
            translateTask,
            Task.Delay(Timeout.Infinite, cancellationTokenSource.Token)
        );
        if (completedTask is not { IsCanceled: true }) return (true, (await translateTask).Translation);
        _ = translateTask.ContinueWith(static task =>
        {
            if (task.Exception != null)
            {
                //ignored
            }
        }, TaskContinuationOptions.ExecuteSynchronously);
        return (false, string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            Guard.IsNotNull(CurrentTranslateItemModel);
            if (!string.IsNullOrEmpty(CurrentTranslateItemModel.TranslatedText))
            {
                var found = _w3Items.First(x => x.Id == CurrentTranslateItemModel.Id);
                Guard.IsNotNull(found);
                found.Text = CurrentTranslateItemModel.TranslatedText;
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(string.Empty),
                    "TranslatedTextInvalid");
            }

            CurrentTranslateItemModel.IsSaved = true;
            Log.Information("Translation saved.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save translation.");
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
                var found = _w3Items.First(x => x.Id == CurrentTranslateItemModel.Id);
                Guard.IsNotNull(found);
                found.Text = CurrentTranslateItemModel.TranslatedText;
            }

            IndexOfItems += indexChange;
            Log.Information("Translator {TranslatorName} moved to {Direction} item (new index: {NewIndex})",
                _translator.Name, indexChange > 0 ? "next" : "previous", IndexOfItems);
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
        if (!_disposedValue)
        {
            if (_cancellationTokenSource == null) return;
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        Log.Information("TranslateContentViewModel is being finalized.");
    }

    partial void OnFormLanguageChanged(ILanguage value)
    {
        Log.Information("The source language has been changed to: {Name}.", value.Name);
    }

    partial void OnToLanguageChanged(ILanguage value)
    {
        Log.Information("The target language has been changed to: {Name}.", value.Name);
    }
}