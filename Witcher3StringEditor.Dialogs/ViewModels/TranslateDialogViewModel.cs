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

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Switch()
    {
        try
        {
            if (!CurrentViewModel.GetIsBusy() ||
                await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationModeSwitch"))
            {
                await CleanupCurrentViewModelAsync();
                await DisposeCurrentViewModelAsync();
                CurrentViewModel = CurrentViewModel is BatchItemsTranslationViewModel
                    ? new SingleItemTranslationViewModel(appSettings, translator, w3StringItems, index)
                    : new BatchItemsTranslationViewModel(appSettings, translator,
                        w3StringItems, index + 1);
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
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
        {
            var found = w3StringItems.First(x => x.TrackingId == item.Id);
            Guard.IsNotNull(found);
            found.Text = item.TranslatedText;
            Log.Information("Auto-saved unsaved changes.");
        }
    }

    private async Task<bool> HandleClosingAsync()
    {
        if (!CurrentViewModel.GetIsBusy() ||
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationDialogClosing"))
        {
            await CleanupCurrentViewModelAsync();
            return false;
        }

        Log.Information("Translation dialog closing cancelled.");
        return true;
    }
}