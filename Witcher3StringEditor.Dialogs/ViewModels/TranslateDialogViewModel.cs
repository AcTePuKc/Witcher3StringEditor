using System.ComponentModel;
using CommandLine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Constants;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IAppSettings appSettings;

    private readonly int index;

    private readonly ITranslator translator;

    private readonly IReadOnlyList<ITrackableW3StringItem> w3StringItems;

    [ObservableProperty] private TranslationViewModelBase currentViewModel;

    [ObservableProperty] private string title = Strings.TranslateDialogTitle;

    public TranslateDialogViewModel(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<ITrackableW3StringItem> w3StringItems,
        int index)
    {
        this.index = index;
        this.translator = translator;
        this.appSettings = appSettings;
        this.w3StringItems = w3StringItems;
        Log.Information("Total items to translate: {Count}.", this.w3StringItems.Count);
        Log.Information("Starting index: {Index}.", index);
        CurrentViewModel = new SingleItemTranslationViewModel(appSettings, translator, this.w3StringItems, index);
    }

    [RelayCommand]
    private async Task DetectLanguage()
    {
        var text = w3StringItems[0].Text;
        var detectedLanguage = await translator.DetectLanguageAsync(text);
        CurrentViewModel.FormLanguage = new Language(detectedLanguage.ISO6391);
    }

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Switch()
    {
        try
        {
            if (!CurrentViewModel.GetIsBusy() ||
                await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                    MessageTokens.TranslationModeSwitch))
            {
                await CleanupCurrentViewModelAsync();
                await DisposeCurrentViewModelAsync();
                var formLange = CurrentViewModel.FormLanguage;
                CurrentViewModel = CurrentViewModel is BatchItemsTranslationViewModel
                    ? new SingleItemTranslationViewModel(appSettings, translator, w3StringItems, index)
                    : new BatchItemsTranslationViewModel(appSettings, translator,
                        w3StringItems, index + 1);
                CurrentViewModel.FormLanguage = formLange;
                Title = CurrentViewModel is BatchItemsTranslationViewModel
                    ? Strings.BatchTranslateDialogTitle
                    : Strings.TranslateDialogTitle;
                Log.Information("Switched translation mode to {Mode}",
                    CurrentViewModel is BatchItemsTranslationViewModel ? "batch" : "single");
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

    private async Task CleanupCurrentViewModelAsync()
    {
        switch (CurrentViewModel)
        {
            case BatchItemsTranslationViewModel { IsBusy: true } batchVm:
                await batchVm.CancelCommand.ExecuteAsync(null);
                break;
            case SingleItemTranslationViewModel singleVm:
                await SaveUnsavedChangesIfNeeded(singleVm);
                break;
        }
    }

    private async Task DisposeCurrentViewModelAsync()
    {
        await CurrentViewModel.DisposeAsync();
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

    private async Task SaveUnsavedChangesIfNeeded(SingleItemTranslationViewModel? translateViewModel)
    {
        if (translateViewModel?.CurrentTranslateItemModel is { IsSaved: false } item
            && !string.IsNullOrWhiteSpace(item.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                MessageTokens.TranslatedTextNoSaved))
        {
            var found = w3StringItems
                .First(x => x.TrackingId == item.Id).Clone();
            var clone = found.Cast<ITrackableW3StringItem>();
            clone.Text = item.TranslatedText;
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<ITrackableW3StringItem>(clone),
                MessageTokens.TranslationSaved);
            Log.Information("Auto-saved unsaved changes.");
        }
    }

    private async Task<bool> HandleClosingAsync()
    {
        if (!CurrentViewModel.GetIsBusy() ||
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                MessageTokens.TranslationDialogClosing))
        {
            await CleanupCurrentViewModelAsync();
            return false;
        }

        Log.Information("Translation dialog closing cancelled.");
        return true;
    }
}