using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using GTranslate;
using GTranslate.Translators;
using FluentResults;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Dialogs.ViewModels;

/// <summary>
///     Base class for translation view models
///     Provides common functionality for both single item and batch translation operations
///     Implements ObservableObject for property change notifications and IAsyncDisposable for async cleanup
/// </summary>
public abstract partial class TranslationViewModelBase : ObservableObject, IAsyncDisposable
{
    /// <summary>
    ///     The translation service used for translating text
    /// </summary>
    private protected readonly ITranslator Translator;

    /// <summary>
    ///     Application settings used for translation context.
    /// </summary>
    private protected readonly IAppSettings AppSettings;

    /// <summary>
    ///     The translation router used to select between legacy and provider flows
    /// </summary>
    private protected readonly ITranslationRouter TranslationRouter;

    /// <summary>
    ///     Post-processor for translation output cleanup.
    /// </summary>
    private protected readonly ITranslationPostProcessor TranslationPostProcessor;

    /// <summary>
    ///     The collection of items to translate
    /// </summary>
    private protected readonly IReadOnlyList<ITrackableW3StringItem> W3StringItems;

    /// <summary>
    ///     The cancellation token source for managing translation operation cancellation
    /// </summary>
    private protected CancellationTokenSource? CancellationTokenSource;

    /// <summary>
    ///     Gets or sets the source language for translation
    /// </summary>
    [ObservableProperty] private ILanguage formLanguage;

    /// <summary>
    ///     Gets or sets the collection of supported languages for the current translator
    /// </summary>
    [ObservableProperty] private IEnumerable<ILanguage> languages;

    /// <summary>
    ///     Gets or sets the target language for translation
    /// </summary>
    [ObservableProperty] private ILanguage toLanguage;

    /// <summary>
    ///     Gets or sets the latest translation status message.
    /// </summary>
    [ObservableProperty] private string statusMessage = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the TranslationViewModelBase class
    /// </summary>
    /// <param name="appSettings">Application settings service</param>
    /// <param name="translator">Translation service</param>
    /// <param name="translationRouter">Translation router service</param>
    /// <param name="w3StringItems">Collection of items to translate</param>
    protected TranslationViewModelBase(IAppSettings appSettings, ITranslator translator,
        ITranslationRouter translationRouter, ITranslationPostProcessor translationPostProcessor,
        IReadOnlyList<ITrackableW3StringItem> w3StringItems)
    {
        AppSettings = appSettings;
        W3StringItems = w3StringItems;
        Translator = translator;
        TranslationRouter = translationRouter;
        TranslationPostProcessor = translationPostProcessor;
        Languages = GetSupportedLanguages(translator);
        FormLanguage = Language.GetLanguage("en");
        ToLanguage = GetPreferredLanguage(appSettings);
    }

    /// <summary>
    ///     Disposes of the view model resources asynchronously
    ///     Must be implemented by derived classes
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public abstract ValueTask DisposeAsync();

    /// <summary>
    ///     Gets a value indicating whether a translation operation is currently in progress
    ///     Must be implemented by derived classes
    /// </summary>
    /// <returns>True if busy, false otherwise</returns>
    public abstract bool GetIsBusy();

    /// <summary>
    ///     Gets the collection of supported languages for a specific translator
    /// </summary>
    /// <param name="translator">The translator to get supported languages for</param>
    /// <returns>A collection of supported languages</returns>
    private static IEnumerable<ILanguage> GetSupportedLanguages(ITranslator translator)
    {
        return translator.Name switch // Return languages based on translator type
        {
            "MicrosoftTranslator" => Language.LanguageDictionary.Values.Where(x => // Microsoft supported languages
                x.SupportedServices.HasFlag(TranslationServices.Microsoft)),
            "GoogleTranslator" => Language.LanguageDictionary.Values.Where(x => // Google supported languages
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            "YandexTranslator" => Language.LanguageDictionary.Values.Where(x => // Yandex supported languages
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            _ => Language.LanguageDictionary.Values // Default to all languages
        };
    }

    /// <summary>
    ///     Gets the preferred language from application settings
    /// </summary>
    /// <param name="appSettings">Application settings service</param>
    /// <returns>The preferred language</returns>
    private static Language GetPreferredLanguage(IAppSettings appSettings)
    {
        var description = typeof(W3Language).GetField(appSettings.PreferredLanguage.ToString())!
            .GetCustomAttribute<DescriptionAttribute>()!.Description; // Get language description
        return description == "es-MX" ? new Language("es") : new Language(description); // Return preferred language
    }

    /// <summary>
    ///     Called when the FormLanguage property changes
    ///     Logs the change in source language
    /// </summary>
    /// <param name="value">The new source language value</param>
    partial void OnFormLanguageChanged(ILanguage value)
    {
        Log.Information("The source language has been changed to: {Name}.", value.Name);
    }

    /// <summary>
    ///     Called when the ToLanguage property changes
    ///     Logs the change in target language
    /// </summary>
    /// <param name="value">The new target language value</param>
    partial void OnToLanguageChanged(ILanguage value)
    {
        Log.Information("The target language has been changed to: {Name}.", value.Name);
    }

    /// <summary>
    ///     Updates the status message based on translation result metadata.
    /// </summary>
    /// <param name="result">The translation result to inspect.</param>
    private protected void UpdateStatusMessage(Result<string> result)
    {
        if (result is null)
        {
            return;
        }

        var status = result.GetStatusMessage();
        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    /// <summary>
    ///     Applies post-processing to a translated string using the current settings context.
    /// </summary>
    /// <param name="input">The translated text to post-process.</param>
    /// <param name="sourceLanguage">The source language.</param>
    /// <param name="targetLanguage">The target language.</param>
    /// <returns>The post-processed text.</returns>
    private protected string ApplyPostProcessing(string input, ILanguage sourceLanguage, ILanguage targetLanguage)
    {
        var context = new TranslationContext(
            sourceLanguage.Name,
            targetLanguage.Name,
            AppSettings.TranslationProviderName,
            AppSettings.TranslationModelName,
            AppSettings.TranslationProfileId,
            AppSettings.UseTerminologyPack,
            AppSettings.UseStyleGuide);
        var processed = TranslationPostProcessor.Process(input, context);
        return string.IsNullOrWhiteSpace(processed) ? input : processed;
    }
}
