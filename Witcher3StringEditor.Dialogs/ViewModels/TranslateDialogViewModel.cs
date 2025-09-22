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

/// <summary>
///     ViewModel for the translate dialog window
///     Manages the translation process and switching between single and batch translation modes
///     Implements IModalDialogViewModel to support dialog result handling
/// </summary>
public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    /// <summary>
    ///     The application settings service
    /// </summary>
    private readonly IAppSettings appSettings;

    /// <summary>
    ///     The starting index for translation
    /// </summary>
    private readonly int index;

    /// <summary>
    ///     The translation service
    /// </summary>
    private readonly ITranslator translator;

    /// <summary>
    ///     The collection of items to translate
    /// </summary>
    private readonly IReadOnlyList<ITrackableW3StringItem> w3StringItems;

    /// <summary>
    ///     Gets or sets the current translation view model (either single or batch)
    /// </summary>
    [ObservableProperty] private TranslationViewModelBase currentViewModel;

    /// <summary>
    ///     Gets or sets the title of the dialog window
    /// </summary>
    [ObservableProperty] private string title = Strings.TranslateDialogTitle;

    /// <summary>
    ///     Initializes a new instance of the TranslateDialogViewModel class
    /// </summary>
    /// <param name="appSettings">Application settings service</param>
    /// <param name="translator">Translation service</param>
    /// <param name="w3StringItems">Collection of items to translate</param>
    /// <param name="index">Starting index for translation</param>
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

    /// <summary>
    ///     Gets the dialog result value
    ///     Returns true to indicate that the dialog was closed successfully
    /// </summary>
    public bool? DialogResult => true;

    /// <summary>
    ///     Detects the language of the first item's text
    ///     Sets the source language based on the detection result
    /// </summary>
    [RelayCommand]
    private async Task DetectLanguage()
    {
        try
        {
            var text = w3StringItems[0].Text;
            var detectedLanguage = await translator.DetectLanguageAsync(text);
            CurrentViewModel.FormLanguage = new Language(detectedLanguage.ISO6391);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to detect language");
        }
    }

    /// <summary>
    ///     Switches between single item and batch translation modes
    ///     Cleans up the current view model and creates a new one of the opposite type
    /// </summary>
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

    /// <summary>
    ///     Cleans up the current view model based on its type
    ///     Cancels any ongoing operations or saves unsaved changes
    /// </summary>
    private async Task CleanupCurrentViewModelAsync()
    {
        switch (CurrentViewModel)
        {
            case BatchItemsTranslationViewModel { IsBusy: true } batchVm: // If batch translation is in progress
                await batchVm.CancelCommand.ExecuteAsync(null); // Cancel the operation
                break;
            case SingleItemTranslationViewModel singleVm: // If single item translation is active
                await SaveUnsavedChangesIfNeeded(singleVm); // Save any unsaved changes
                break;
        }
    }

    /// <summary>
    ///     Disposes of the current view model
    /// </summary>
    private async Task DisposeCurrentViewModelAsync()
    {
        await CurrentViewModel.DisposeAsync();
    }

    /// <summary>
    ///     Handles the dialog closing event
    ///     Prevents closing if a translation operation is in progress and the user cancels
    /// </summary>
    /// <param name="e">Cancel event arguments</param>
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

    /// <summary>
    ///     Handles the dialog closed event
    ///     Disposes of the current view model
    /// </summary>
    [RelayCommand]
    private async Task Closed()
    {
        await DisposeCurrentViewModelAsync();
    }

    /// <summary>
    ///     Saves any unsaved changes in the single item translation view model
    /// </summary>
    /// <param name="translateViewModel">The single item translation view model</param>
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

    /// <summary>
    ///     Handles the dialog closing process
    ///     Cleans up the current view model if not busy or if the user confirms
    /// </summary>
    /// <returns>True to cancel closing, false to allow closing</returns>
    private async Task<bool> HandleClosingAsync()
    {
        if (!CurrentViewModel.GetIsBusy() || // Allow closing if not busy
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), // Or if user confirms
                MessageTokens.TranslationDialogClosing))
        {
            await CleanupCurrentViewModelAsync(); // Clean up before closing
            return false; // Allow the dialog to close
        }

        Log.Information("Translation dialog closing cancelled."); // Log if closing is prevented
        return true; // Prevent the dialog from closing
    }
}