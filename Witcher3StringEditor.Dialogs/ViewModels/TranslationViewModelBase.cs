using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public abstract partial class TranslationViewModelBase : ObservableObject, IAsyncDisposable
{
    private protected readonly ITranslator Translator;
    private protected readonly IReadOnlyList<ITrackableW3StringItem> W3StringItems;
    private protected CancellationTokenSource? CancellationTokenSource;
    [ObservableProperty] private ILanguage formLanguage;
    [ObservableProperty] private IEnumerable<ILanguage> languages;
    [ObservableProperty] private ILanguage toLanguage;

    protected TranslationViewModelBase(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<ITrackableW3StringItem> w3StringItems)
    {
        W3StringItems = [.. w3StringItems];
        Translator = translator;
        Languages = GetSupportedLanguages(translator);
        FormLanguage = Language.GetLanguage("en");
        ToLanguage = GetPreferredLanguage(appSettings);
    }

    public abstract ValueTask DisposeAsync();

    public abstract bool GetIsBusy();

    private static IEnumerable<ILanguage> GetSupportedLanguages(ITranslator translator)
    {
        return translator.Name switch
        {
            "MicrosoftTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Microsoft)),
            "GoogleTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            "YandexTranslator" => Language.LanguageDictionary.Values.Where(x =>
                x.SupportedServices.HasFlag(TranslationServices.Google)),
            _ => Language.LanguageDictionary.Values
        };
    }

    private static Language GetPreferredLanguage(IAppSettings appSettings)
    {
        var description = typeof(W3Language).GetField(appSettings.PreferredLanguage.ToString())!
            .GetCustomAttribute<DescriptionAttribute>()!.Description;
        return description == "es-MX" ? new Language("es") : new Language(description);
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