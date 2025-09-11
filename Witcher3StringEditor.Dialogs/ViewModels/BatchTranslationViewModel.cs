using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public sealed partial class BatchTranslationViewModel : TranslationViewModelBase
{
    [ObservableProperty] private int _endIndex;

    [ObservableProperty] private int _endIndexMin;

    [ObservableProperty] private int _failureCount;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    [ObservableProperty] private int _maxValue;

    [ObservableProperty] private int _pendingCount;

    [ObservableProperty] private int _startIndex;

    [ObservableProperty] private int _successCount;

    public BatchTranslationViewModel(IAppSettings appSettings, ITranslator translator,
        IReadOnlyList<ITrackableW3StringItem> w3Items, int startIndex) : base(appSettings, translator, w3Items)
    {
        StartIndex = startIndex;
        EndIndex = MaxValue = W3Items.Count;
        Log.Information("BatchTranslateContentViewModel is initialized.");
    }

    private bool CanCancel => IsBusy;

    private bool CanStart => !IsBusy;

    public override async ValueTask DisposeAsync()
    {
        if (CancellationTokenSource != null)
        {
            if (!CancellationTokenSource.IsCancellationRequested)
                await CancellationTokenSource.CancelAsync();
            CancellationTokenSource.Dispose();
        }

        Log.Information("BatchTranslateContentViewModel is being disposed.");
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
        try
        {
            await ExecuteBatchTranslation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExecuteBatchTranslation()
    {
        IsBusy = true;
        ResetTranslationCounts();
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
        await ProcessTranslationItems(W3Items.Skip(StartIndex - 1).Take(PendingCount),
            ToLanguage, FormLanguage, CancellationTokenSource.Token);
    }

    private async Task ProcessTranslationItems(IEnumerable<IW3StringItem> items, ILanguage toLanguage,
        ILanguage fromLanguage,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
            if (!cancellationToken.IsCancellationRequested)
            {
                await ProcessSingleItem(item, toLanguage, fromLanguage);
                PendingCount--;
            }
            else
            {
                Log.Information("The batch translation has been cancelled.");
                break;
            }
    }

    private async Task ProcessSingleItem(IW3StringItem item, ILanguage toLanguage, ILanguage fromLanguage)
    {
        try
        {
            var (result, translation) = await TranslateItem(Translator, item.Text, toLanguage, fromLanguage);
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
                Translator.Name, ex.Message);
            FailureCount++;
        }
    }

    private static async Task<(bool, string)> TranslateItem(ITranslator translator, string? text, ILanguage tLanguage,
        ILanguage fLanguage)
    {
        if (string.IsNullOrWhiteSpace(text)) return (true, string.Empty);
        var translation = (await translator.TranslateAsync(text, tLanguage, fLanguage)).Translation;
        if (IsTranslationValid(translation)) return (true, translation);
        LogEmptyTranslationResult(translator.Name);
        return (false, string.Empty);
    }

    private static bool IsTranslationValid(string translation)
    {
        return !string.IsNullOrWhiteSpace(translation);
    }

    private static void LogEmptyTranslationResult(string translatorName)
    {
        Log.Error("The translator: {Name} returned empty data.", translatorName);
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task Cancel()
    {
        if (CancellationTokenSource != null)
            await CancellationTokenSource.CancelAsync();
    }
}