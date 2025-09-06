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

    private readonly IReadOnlyList<IEditW3Item> w3Items;

    [ObservableProperty] private object currentViewModel;

    [ObservableProperty] private string title = Strings.TranslateDialogTitle;

    public TranslateDialogViewModel(IAppSettings appSettings, ITranslator translator,
        IEnumerable<IEditW3Item> w3Items,
        int index)
    {
        this.w3Items = [.. w3Items];
        this.index = index;
        this.appSettings = appSettings;
        this.translator = translator;
        Log.Information("Total items to translate: {Count}.", this.w3Items.Count);
        Log.Information("Starting index: {Index}.", index);
        CurrentViewModel = new TranslateContentViewModel(appSettings, translator, this.w3Items, index);
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
                if (CurrentViewModel is BatchTranslateContentViewModel { IsBusy: true } batchVm)
                    await batchVm.CancelCommand.ExecuteAsync(null);

                await SaveUnsavedChangesIfNeeded(CurrentViewModel as TranslateContentViewModel);
                await ((IAsyncDisposable)CurrentViewModel).DisposeAsync();
                CurrentViewModel = CurrentViewModel is BatchTranslateContentViewModel
                    ? new TranslateContentViewModel(appSettings, translator, w3Items, index)
                    : new BatchTranslateContentViewModel(appSettings, translator,
                        w3Items, index + 1);
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
        await ((IAsyncDisposable)CurrentViewModel).DisposeAsync();
    }

    private async Task SaveUnsavedChangesIfNeeded(TranslateContentViewModel? translateViewModel)
    {
        if (translateViewModel?.CurrentTranslateItemModel is { IsSaved: false } item
            && !string.IsNullOrWhiteSpace(item.TranslatedText)
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
        {
            var found = w3Items.First(x => x.Id == item.Id);
            Guard.IsNotNull(found);
            found.Text = item.TranslatedText;
            Log.Information("Auto-saved unsaved changes for item {ItemId}", item.Id);
        }
    }

    private async Task<bool> HandleClosingAsync()
    {
        if (CurrentViewModel is not (TranslateContentViewModel { IsBusy: true }
                or BatchTranslateContentViewModel { IsBusy: true }) || await WeakReferenceMessenger.Default.Send(
                new AsyncRequestMessage<bool>(),
                "TranslationDialogClosing"))
        {
            if (CurrentViewModel is BatchTranslateContentViewModel { IsBusy: true } batchVm)
            {
                await batchVm.CancelCommand.ExecuteAsync(null);
                return false;
            }

            if (CurrentViewModel is not TranslateContentViewModel singleVm) return false;
            await SaveUnsavedChangesIfNeeded(singleVm);
            return false;
        }

        Log.Information("Translation dialog closing cancelled.");
        return true;
    }
}