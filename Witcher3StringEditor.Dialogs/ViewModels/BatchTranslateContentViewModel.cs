using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public sealed partial class BatchTranslateContentViewModel : ObservableObject, IAsyncDisposable
{
    private readonly ITranslator _translator;
    private readonly IReadOnlyCollection<IW3Item> _w3Items;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty] private int _endIndex;

    [ObservableProperty] private int _endIndexMin;

    [ObservableProperty] private int _failureCount;

    [ObservableProperty] private ILanguage _formLanguage;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    [ObservableProperty] private IEnumerable<ILanguage> _languages;

    [ObservableProperty] private int _maxValue;

    [ObservableProperty] private int _pendingCount;

    [ObservableProperty] private int _startIndex;

    [ObservableProperty] private int _successCount;

    [ObservableProperty] private ILanguage _toLanguage;

    public BatchTranslateContentViewModel(IAppSettings appSettings, ITranslator translator,
        IEnumerable<IW3Item> w3Items, int startIndex)
    {
        _translator = translator;
        _w3Items = [.. w3Items];
        Languages = Language.LanguageDictionary.Values
            .Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft));
        StartIndex = startIndex;
        EndIndex = MaxValue = _w3Items.Count;
        FormLanguage = Language.GetLanguage("en");
        ToLanguage = appSettings.PreferredLanguage switch
        {
            W3Language.Br => Language.GetLanguage("pt"),
            W3Language.Cn => Language.GetLanguage("zh-CN"),
            W3Language.Esmx => Language.GetLanguage("es"),
            W3Language.Cz => Language.GetLanguage("cs"),
            W3Language.Jp => Language.GetLanguage("ja"),
            W3Language.Kr => Language.GetLanguage("ko"),
            W3Language.Zh => Language.GetLanguage("zh-TW"),
            _ => Language.GetLanguage(Enum.GetName(appSettings.PreferredLanguage) ?? "en")
        };
        Log.Information("BatchTranslateContentViewModel is initialized.");
    }

    private bool CanCancel => IsBusy;

    private bool CanStart => !IsBusy && PendingCount > 0;

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource != null)
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
        }

        Log.Information("BatchTranslateContentViewModel is being disposed.");
    }

    partial void OnFormLanguageChanged(ILanguage value)
    {
        Log.Information("The source language has been changed to: {Name}.", value.Name);
    }

    partial void OnToLanguageChanged(ILanguage value)
    {
        Log.Information("The target language has been changed to: {Name}.", value.Name);
    }

    partial void OnStartIndexChanged(int value)
    {
        EndIndexMin = value > MaxValue ? MaxValue : value;
        if (!IsBusy) ResetTranslationCounts();
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnEndIndexChanged(int value)
    {
        if (!IsBusy) ResetTranslationCounts();
    }

    private void ResetTranslationCounts()
    {
        SuccessCount = 0;
        FailureCount = 0;
        PendingCount = EndIndex - StartIndex + 1;
    }

    partial void OnIsBusyChanged(bool value)
    {
        Log.Information("The batch translation is in progress: {0}.", value);
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var tLanguage = ToLanguage;
        var fLanguage = FormLanguage;
        ResetTranslationCounts();
        foreach (var item in _w3Items.Skip(StartIndex - 1).Take(PendingCount))
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var (result, translation) = await TranslateItem(_translator, item.Text, tLanguage, fLanguage);
                    if (result)
                    {
                        item.Text = translation;
                        SuccessCount++;
                    }
                    else
                    {
                        FailureCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "The translator: {Name} returned an error. Exception: {ExceptionMessage}",
                        _translator.Name, ex.Message);
                    FailureCount++;
                }

                PendingCount--;
            }
            else
            {
                Log.Information("Batch translations are canceled by the user.");
                return;
            }

        IsBusy = false;
    }

    private static async Task<(bool, string)> TranslateItem(ITranslator translator, string text, ILanguage tLanguage,
        ILanguage fLanguage)
    {
        var translation = (await translator.TranslateAsync(text, tLanguage, fLanguage)).Translation;
        if (!string.IsNullOrWhiteSpace(translation)) return (true, translation);

        Log.Error("The translator: {Name} returned empty data.", translator.Name);
        return (false, string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task Cancel()
    {
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            IsBusy = false;
        }
    }
}