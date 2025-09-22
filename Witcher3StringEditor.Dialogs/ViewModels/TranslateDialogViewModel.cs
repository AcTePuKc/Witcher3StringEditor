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
            if (!CurrentViewModel.GetIsBusy() || // Check if not busy
                await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), // Or user confirms switch
                    MessageTokens.TranslationModeSwitch))
            {
                await CleanupCurrentViewModelAsync(); // Clean up current view model
                await DisposeCurrentViewModelAsync(); // Dispose current view model
                var formLange = CurrentViewModel.FormLanguage; // Save current source language
                CurrentViewModel = CurrentViewModel is BatchItemsTranslationViewModel // Switch view model type
                    ? new SingleItemTranslationViewModel(appSettings, translator, w3StringItems, index)
                    : new BatchItemsTranslationViewModel(appSettings, translator,
                        w3StringItems, index + 1);
                CurrentViewModel.FormLanguage = formLange; // Restore source language
                Title = CurrentViewModel is BatchItemsTranslationViewModel // Update dialog title
                    ? Strings.BatchTranslateDialogTitle
                    : Strings.TranslateDialogTitle;
                Log.Information("Switched translation mode to {Mode}", // Log the mode switch
                    CurrentViewModel is BatchItemsTranslationViewModel ? "batch" : "single");
            }
            else
            {
                Log.Information("Translation mode switch cancelled."); // Log if switch is cancelled
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to switch translation mode"); // Log any errors
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
            e.Cancel = await HandleClosingAsync(); // Determine if closing should be cancelled
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during dialog closing"); // Log any errors
            e.Cancel = false; // Allow closing if an error occurs
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
        if (translateViewModel?.CurrentTranslateItemModel is
                { IsSaved: false } item // Check if there are unsaved changes
            && !string.IsNullOrWhiteSpace(item.TranslatedText) // And translated text exists
            && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), // Confirm with user
                MessageTokens.TranslatedTextNoSaved))
        {
            var found = w3StringItems // Find the original item
                .First(x => x.TrackingId == item.Id).Clone();
            var clone = found.Cast<ITrackableW3StringItem>(); // Cast to correct type
            clone.Text = item.TranslatedText; // Update with translated text
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<ITrackableW3StringItem>(clone), // Send via messaging
                MessageTokens.TranslationSaved);
            Log.Information("Auto-saved unsaved changes."); // Log the auto-save
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