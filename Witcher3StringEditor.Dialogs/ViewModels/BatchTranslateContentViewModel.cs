using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTranslate;
using GTranslate.Translators;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class BatchTranslateContentViewModel : ObservableObject, IDisposable
{
    private readonly ITranslator translator;
    private readonly IReadOnlyList<IW3Item> w3Items;
    private CancellationTokenSource? cancellationTokenSource;
    private bool disposedValue;

    [ObservableProperty] private int endIndex;

    [ObservableProperty] private int endIndexMin;

    [ObservableProperty] private int failureCount;

    [ObservableProperty] private ILanguage formLanguage;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isBusy;

    [ObservableProperty] private IEnumerable<ILanguage> languages;

    [ObservableProperty] private int maxValue;

    [ObservableProperty] private int pendingCount;

    [ObservableProperty] private int startIndex;

    [ObservableProperty] private int successCount;

    [ObservableProperty] private ILanguage toLanguage;

    public BatchTranslateContentViewModel(IAppSettings appSettings, ITranslator translator,
        IEnumerable<IW3Item> w3Items, int startIndex)
    {
        this.translator = translator;
        this.w3Items = [.. w3Items];
        Languages = Language.LanguageDictionary.Values
            .Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft));
        StartIndex = startIndex;
        EndIndex = MaxValue = this.w3Items.Count;
        FormLanguage = Language.GetLanguage("en");
        ToLanguage = appSettings.PreferredLanguage switch
        {
            W3Language.br => Language.GetLanguage("pt"),
            W3Language.cn => Language.GetLanguage("zh-CN"),
            W3Language.esmx => Language.GetLanguage("es"),
            W3Language.cz => Language.GetLanguage("cs"),
            W3Language.jp => Language.GetLanguage("ja"),
            W3Language.kr => Language.GetLanguage("ko"),
            W3Language.zh => Language.GetLanguage("zh-TW"),
            _ => Language.GetLanguage(Enum.GetName(appSettings.PreferredLanguage) ?? "en")
        };
    }

    private bool CanCancel => IsBusy;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    partial void OnStartIndexChanged(int value)
    {
        EndIndexMin = value > MaxValue ? MaxValue : value;
    }

    partial void OnIsBusyChanged(bool value)
    {
        Log.Information("The batch translation is in progress: {0}.", value);
    }

    [RelayCommand]
    private async Task Start()
    {
        IsBusy = true;
        SuccessCount = 0;
        FailureCount = 0;
        PendingCount = EndIndex - StartIndex + 1;
        cancellationTokenSource = new CancellationTokenSource();
        var tLanguage = ToLanguage;
        var fLanguage = FormLanguage;
        foreach (var item in w3Items.Skip(StartIndex - 1).Take(PendingCount))
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var translation = (await translator.TranslateAsync(item.Text, tLanguage, fLanguage)).Translation;
                    if (!string.IsNullOrWhiteSpace(translation))
                    {
                        item.Text = translation;
                        SuccessCount++;
                    }
                    else
                    {
                        FailureCount++;
                        Log.Error("The translator: {Name} returned empty data.", translator.Name);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Translation error occurred.");
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

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task Cancel()
    {
        if (IsBusy && cancellationTokenSource != null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
            IsBusy = false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing && cancellationTokenSource != null)
        {
            if(!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
            cancellationTokenSource?.Dispose();
        }

        disposedValue = true;
    }

    ~BatchTranslateContentViewModel()
    {
        Dispose(false);
    }
}