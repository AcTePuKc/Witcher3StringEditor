using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using System.ComponentModel;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class BatchTranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private CancellationTokenSource? cancellationTokenSource;

    private readonly ITranslator translator;

    [ObservableProperty]
    private ILanguage toLanguage;

    [ObservableProperty]
    private ILanguage formLanguage;

    [ObservableProperty]
    private int maxValue;

    [ObservableProperty]
    private int startIndex;

    [ObservableProperty]
    private int endIndex;

    [ObservableProperty]
    private int endIndexMin;

    partial void OnStartIndexChanged(int value) 
        => EndIndexMin = value > MaxValue ? MaxValue : value;

    [ObservableProperty]
    private int successCount;

    [ObservableProperty]
    private int failureCount;

    [ObservableProperty]
    private int pendingCount;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isBusy;

    partial void OnIsBusyChanged(bool value) 
        => WeakReferenceMessenger.Default.Send(new NotificationMessage<bool>(value), "TranslatorIsBusy");

    [ObservableProperty]
    private bool isAiTranslator;

    public bool? DialogResult => true;

    [ObservableProperty]
    private IEnumerable<ILanguage> languages;

    private readonly IEnumerable<IW3Item> w3Items;

    public BatchTranslateDialogViewModel(IEnumerable<IW3Item> w3Items,
                                         int startIndex,
                                         IAppSettings appSettings,
                                         ITranslator translator)
    {
        this.translator = translator;
        this.w3Items = w3Items;
        IsAiTranslator = translator is not MicrosoftTranslator;
        Languages = IsAiTranslator ? Language.LanguageDictionary.Values : Language.LanguageDictionary.Values
            .Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft));
        StartIndex = startIndex;
        MaxValue = this.w3Items.Count();
        EndIndex = MaxValue + 1;
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

    [RelayCommand]
    private async Task Start()
    {
        IsBusy = true;
        SuccessCount = 0; 
        FailureCount = 0;
        PendingCount = EndIndex - StartIndex + 1;
        cancellationTokenSource = new CancellationTokenSource();
        var tLanguage = ToLanguage; var fLanguage = FormLanguage;
        foreach (var item in w3Items.Skip(StartIndex - 1).Take(PendingCount))
        {
            if (cancellationTokenSource.IsCancellationRequested) return;
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
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Translation error occurred.");
                FailureCount++;
            }
            PendingCount--;
        }
        IsBusy = false;
    }

    private bool CanCancel => IsBusy;

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

    [RelayCommand]
    private async Task Closing(CancelEventArgs e)
    {
        if (IsBusy && cancellationTokenSource != null)
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BatchTranslateDialogClosing"))
            {
                e.Cancel = true;
            }
            else
            {
                await cancellationTokenSource.CancelAsync();
                IsBusy = false;
            }
        }
    }
}