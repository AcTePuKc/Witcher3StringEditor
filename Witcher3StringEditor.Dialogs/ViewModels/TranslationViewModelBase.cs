using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public abstract partial class TranslationViewModelBase : ObservableObject
{
    protected readonly ITranslator Translator;
    protected readonly IReadOnlyCollection<IW3StringItem> W3Items;
    protected CancellationTokenSource? CancellationTokenSource;

    
    [ObservableProperty] private ILanguage _formLanguage;
    [ObservableProperty] private IEnumerable<ILanguage> _languages;
    [ObservableProperty] private ILanguage _toLanguage;

    protected TranslationViewModelBase(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<IW3StringItem> w3Items) 
    {
        Translator = translator;
        W3Items = [.. w3Items];
        Languages = GetSupportedLanguages(translator);
        FormLanguage = Language.GetLanguage("en");
        ToLanguage = GetPreferredLanguage(appSettings);
        Log.Information("TranslateContentViewModel initialized.");
    }

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