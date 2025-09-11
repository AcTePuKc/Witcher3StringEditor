using System.ComponentModel;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IAppSettings _appSettings;

    private readonly int _index;

    private readonly ITranslator _translator;

    private readonly IReadOnlyList<ITrackableW3StringItem> _w3Items;

    [ObservableProperty] private object _currentViewModel;

    [ObservableProperty] private string _title = Strings.TranslateDialogTitle;

    public TranslateDialogViewModel(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<ITrackableW3StringItem> w3Items,
        int index)
    {
        _index = index;
        _w3Items = w3Items;
        _translator = translator;
        _appSettings = appSettings;
        Log.Information("Total items to translate: {Count}.", _w3Items.Count);
        Log.Information("Starting index: {Index}.", index);
        CurrentViewModel = new TranslateContentViewModel(appSettings, translator, _w3Items, index);
    }

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Switch()
    {
        try
        {
            if (CurrentViewModel is not (TranslateContentViewModel { IsBusy: true }
                    or BatchTranslateContentViewModel { IsBusy: true }) ||
                await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationModeSwitch"))
            {
                await HandleCurrentViewModelBeforeSwitchAsync();
                await DisposeCurrentViewModelAsync();
                CurrentViewModel = CurrentViewModel is BatchTranslateContentViewModel
                    ? new TranslateContentViewModel(_appSettings, _translator, _w3Items, _index)
                    : new BatchTranslateContentViewModel(_appSettings, _translator,
                        _w3Items, _index + 1);
                Title = CurrentViewModel is BatchTranslateContentViewModel
                    ? Strings.BatchTranslateDialogTitle
                    : Strings.TranslateDialogTitle;
                Log.Information("Switched translation mode to {Mode}",
                    CurrentViewModel is BatchTranslateContentViewModel ? "batch" : "single");
            }
            else
            {
                Log.Information("Translation mode switch cancelled.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to switch translation mode");
        }
    }

    private async Task HandleCurrentViewModelBeforeSwitchAsync()
    {
        switch (CurrentViewModel)
        {
            case BatchTranslateContentViewModel { IsBusy: true } batchVm:
                await batchVm.CancelCommand.ExecuteAsync(null);
                break;
            case TranslateContentViewModel singleVm:
                await SaveUnsavedChangesIfNeeded(singleVm);
                break;
        }
    }

    private async Task DisposeCurrentViewModelAsync()
    {
        if (CurrentViewModel is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
    }

    [RelayCommand]
    private async Task Closing(CancelEventArgs e)
    {
        try
        {
            e.Cancel = await HandleClosingAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during dialog closing");
            e.Cancel = false;
        }
    }

    [RelayCommand]
    private async Task Closed()
    {
        await DisposeCurrentViewModelAsync();
    }

    private async Task SaveUnsavedChangesIfNeeded(TranslateContentViewModel? translateViewModel)
    {
        if (translateViewModel?.CurrentTranslateItemModel is { IsSaved: false } item
            && !string.IsNullOrWhiteSpace(item.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
        {
            var found = _w3Items.First(x => x.TrackingId == item.Id);
            Guard.IsNotNull(found);
            found.Text = item.TranslatedText;
            Log.Information("Auto-saved unsaved changes.");
        }
    }

    private async Task<bool> HandleClosingAsync()
    {
        if (CurrentViewModel is not (TranslateContentViewModel { IsBusy: true }
                or BatchTranslateContentViewModel { IsBusy: true }) || await WeakReferenceMessenger.Default.Send(
                new AsyncRequestMessage<bool>(),
                "TranslationDialogClosing"))
        {
            await HandleCurrentViewModelBeforeSwitchAsync();
            return false;
        }

        Log.Information("Translation dialog closing cancelled.");
        return true;
    }
}