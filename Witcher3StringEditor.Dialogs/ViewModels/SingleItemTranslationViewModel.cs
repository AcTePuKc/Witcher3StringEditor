using System.Globalization;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public sealed partial class SingleItemTranslationViewModel : TranslationViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int currentItemIndex = -1;

    [ObservableProperty] private TranslateItemModel? currentTranslateItemModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    public SingleItemTranslationViewModel(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<ITrackableW3StringItem> w3StringItems,
        int index) : base(appSettings, translator, w3StringItems)
    {
        CurrentItemIndex = index;
        Log.Information("Initializing SingleItemTranslationViewModel.");
    }

    private bool CanSave => !IsBusy;

    private bool CanPrevious => CurrentItemIndex > 0 && !IsBusy;

    private bool CanNext => CurrentItemIndex < W3StringItems.Count - 1 && !IsBusy;

    public override bool GetIsBusy()
    {
        return IsBusy;
    }

    public override async ValueTask DisposeAsync()
    {
        if (CancellationTokenSource != null)
        {
            if (!CancellationTokenSource.IsCancellationRequested)
                await CancellationTokenSource.CancelAsync();
            CancellationTokenSource.Dispose();
        }

        Log.Information("SingleItemTranslationViewModel is being disposed.");
    }

    partial void OnCurrentItemIndexChanged(int value)
    {
        var selectedItem = W3StringItems[value];
        CurrentTranslateItemModel = new TranslateItemModel { Id = selectedItem.TrackingId, Text = selectedItem.Text };
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
            if (string.IsNullOrWhiteSpace(CurrentTranslateItemModel?.TranslatedText))
            {
                IsBusy = true;
                Guard.IsNotNullOrWhiteSpace(CurrentTranslateItemModel?.Text);
                CancellationTokenSource?.Dispose();
                CancellationTokenSource = new CancellationTokenSource();
                CurrentTranslateItemModel.TranslatedText = string.Empty;
                Log.Information("Starting translation.");
                var translationResult = await ExecuteTranslationTask(CurrentTranslateItemModel.Text,
                    ToLanguage, FormLanguage, CancellationTokenSource);
                if (translationResult.IsSuccess)
                {
                    Guard.IsNotNullOrWhiteSpace(translationResult.Value);
                    CurrentTranslateItemModel.TranslatedText = translationResult.Value;
                    Log.Information("Translation completed.");
                }
                else
                {
                    Log.Error("Translation operation was cancelled.");
                }
            }
            else
            {
                _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationNotEmpty");
            }
        }
        catch (Exception ex)
        {
            const string errorMessage = "The translator: {0} returned an error. Exception: {1}";
            _ = WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<string>(string.Format(CultureInfo.InvariantCulture, errorMessage,
                    Translator.Name, ex.Message)),
                "TranslateError");
            Log.Error(ex, errorMessage, Translator.Name, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Result<string>> ExecuteTranslationTask(string text,
        ILanguage toLanguage, ILanguage formLanguage, CancellationTokenSource cancellationTokenSource)
    {
        var translateTask = Translator.TranslateAsync(text, toLanguage, formLanguage);
        var completedTask = await Task.WhenAny(
            translateTask,
            Task.Delay(Timeout.Infinite, cancellationTokenSource.Token)
        );
        if (completedTask is not { IsCanceled: true })
            return Result.Ok((await translateTask).Translation);
        _ = translateTask.ContinueWith(static task =>
        {
            if (task.Exception != null)
            {
                //ignored
            }
        }, TaskContinuationOptions.ExecuteSynchronously);
        return Result.Fail(string.Empty);    
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(CurrentTranslateItemModel?.TranslatedText))
                SaveTranslation();
            else
                _ = WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(string.Empty),
                    "TranslatedTextInvalid");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save translation.");
        }
    }

    private async Task Navigate(int direction)
    {
        try
        {
            if (CurrentTranslateItemModel is { IsSaved: false }
                && !string.IsNullOrWhiteSpace(CurrentTranslateItemModel.TranslatedText)
                && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                    "TranslatedTextNoSaved"))
                SaveTranslation();
            CurrentItemIndex += direction;
            Log.Information("Translator {TranslatorName} moved to {Direction} item (new index: {NewIndex})",
                Translator.Name, direction > 0 ? "next" : "previous", CurrentItemIndex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to move to {Direction} item",
                direction > 0 ? "next" : "previous");
        }
    }

    private void SaveTranslation()
    {
        Guard.IsNotNull(CurrentTranslateItemModel);
        var found = W3StringItems
            .First(x => x.TrackingId == CurrentTranslateItemModel?.Id);
        found.Text = CurrentTranslateItemModel.TranslatedText;
        CurrentTranslateItemModel.IsSaved = true;
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
}