using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IAppSettings appSettings;

    private readonly int index;
    private readonly NotificationRecipient<bool> recipient = new();

    private readonly ITranslator translator;

    private readonly IEnumerable<IW3Item> w3Items;

    [ObservableProperty] private object currentViewModel;

    [ObservableProperty] private bool switchIsEnabled = true;

    [ObservableProperty] private string title = Strings.TranslateDialogTitle;

    public TranslateDialogViewModel(IAppSettings appSettings, ITranslator translator, IEnumerable<IW3Item> w3Items,
        int index)
    {
        this.w3Items = w3Items;
        this.index = index;
        this.appSettings = appSettings;
        this.translator = translator;
        CurrentViewModel = new TranslateContentViewModel(appSettings, translator, this.w3Items, index);
        WeakReferenceMessenger.Default.Register<NotificationRecipient<bool>, NotificationMessage<bool>, string>(
            recipient, "TranslatorIsBatchTranslating", (r, m) =>
            {
                r.Receive(m);
                SwitchIsEnabled = !m.Message;
            });
    }

    public bool? DialogResult => true;

    [RelayCommand]
    private async Task Switch()
    {
        if (CurrentViewModel is not TranslateContentViewModel { IsBusy: true }
            || await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationModeSwitch"))
        {
            if (CurrentViewModel is TranslateContentViewModel
                {
                    CurrentTranslateItemModel.IsSaved: false
                } translateViewModel
                && !string.IsNullOrWhiteSpace(translateViewModel.CurrentTranslateItemModel.TranslatedText)
                && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
                w3Items.First(x => x.Id == translateViewModel.CurrentTranslateItemModel?.Id).Text =
                    translateViewModel.CurrentTranslateItemModel.TranslatedText;
            CurrentViewModel = CurrentViewModel is TranslateContentViewModel
                ? new BatchTranslateContentViewModel(appSettings, translator, w3Items, index + 1)
                : new TranslateContentViewModel(appSettings, translator, w3Items, index);
            Title = CurrentViewModel.GetType() == typeof(BatchTranslateContentViewModel)
                ? Strings.BatchTranslateDialogTitle
                : Strings.TranslateDialogTitle;
            Log.Information("Switch translation mode to {0} mode.",
                CurrentViewModel is BatchTranslateContentViewModel ? "batch" : "single");
        }
    }

    [RelayCommand]
    private async Task Closing(CancelEventArgs e)
    {
        switch (CurrentViewModel)
        {
            case TranslateContentViewModel { IsBusy: true } or BatchTranslateContentViewModel { IsBusy: true }
                when !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                    "TranslationDialogClosing"):
                e.Cancel = true;
                break;

            case BatchTranslateContentViewModel { IsBusy: true } batchTranslateViewModel:
                await batchTranslateViewModel.CancelCommand.ExecuteAsync(null);
                break;

            case TranslateContentViewModel translateViewModel:
            {
                if (translateViewModel.CurrentTranslateItemModel is { IsSaved: false }
                    && !string.IsNullOrWhiteSpace(translateViewModel.CurrentTranslateItemModel.TranslatedText)
                    && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                        "TranslatedTextNoSaved"))
                    w3Items.First(x => x.Id == translateViewModel.CurrentTranslateItemModel.Id).Text =
                        translateViewModel.CurrentTranslateItemModel.TranslatedText;
                break;
            }
        }
    }

    [RelayCommand]
    private void Closed()
    {
        WeakReferenceMessenger.Default.UnregisterAll(recipient);
    }
}