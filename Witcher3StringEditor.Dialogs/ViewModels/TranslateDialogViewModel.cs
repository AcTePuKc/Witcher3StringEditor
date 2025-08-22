using System.ComponentModel;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IAppSettings appSettings;

    private readonly int index;

    private readonly ITranslator translator;

    private readonly IReadOnlyList<IW3Item> w3Items;

    [ObservableProperty] private object currentViewModel;

    [ObservableProperty] private string title = Strings.TranslateDialogTitle;

    public TranslateDialogViewModel(IAppSettings appSettings, ITranslator translator, IEnumerable<IW3Item> w3Items,
        int index)
    {
        this.w3Items = [.. w3Items];
        this.index = index;
        this.appSettings = appSettings;
        this.translator = translator;
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
                CurrentViewModel = CurrentViewModel is BatchTranslateContentViewModel
                    ? new TranslateContentViewModel(appSettings, translator, w3Items, index)
                    : new BatchTranslateContentViewModel(appSettings, translator, w3Items, index + 1);
                Title = CurrentViewModel is BatchTranslateContentViewModel
                    ? Strings.BatchTranslateDialogTitle
                    : Strings.TranslateDialogTitle;
                Log.Information("Switched translation mode to {Mode}",
                    CurrentViewModel is BatchTranslateContentViewModel ? "batch" : "single");
            }
            else
            {
                Log.Information("Translation mode switch cancelled (busy or user declined)");
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
        switch (CurrentViewModel)
        {
            case TranslateContentViewModel { IsBusy: true } or BatchTranslateContentViewModel { IsBusy: true }
                when !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                    "TranslationDialogClosing"):
                Log.Information("Translation dialog closing cancelled (busy)");
                return true;
            case BatchTranslateContentViewModel { IsBusy: true } batchVm:
                await batchVm.CancelCommand.ExecuteAsync(null);
                return false;
            case TranslateContentViewModel singleVm:
                await SaveUnsavedChangesIfNeeded(singleVm);
                return false;
            default:
                return false;
        }
    }
}