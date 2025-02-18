using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using System.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class TranslateDialogViewModel : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    [ObservableProperty]
    private object currentViewModel;

    private readonly int index;

    private readonly IEnumerable<IW3Item> w3Items;

    private readonly IAppSettings appSettings;

    private readonly ITranslator translator;

    [RelayCommand]
    private async Task Closing(CancelEventArgs e)
    {
        switch (CurrentViewModel)
        {
            case TranslateContentViewModel { IsBusy: true } when !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationDialogClosing"):
                e.Cancel = true;
                break;

            case TranslateContentViewModel translateViewModel:
                {
                    if (translateViewModel.CurrentTranslateItemModel is { IsSaved: false }
                        && !string.IsNullOrWhiteSpace(translateViewModel.CurrentTranslateItemModel.TranslatedText)
                        && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
                        w3Items.First(x => x.Id == translateViewModel.CurrentTranslateItemModel.Id).Text = translateViewModel.CurrentTranslateItemModel.TranslatedText;
                    break;
                }
            case BatchTranslateContentViewModel { IsBusy: true } when !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationDialogClosing"):
                e.Cancel = true;
                break;

            case BatchTranslateContentViewModel { IsBusy: true } batchTranslateViewModel:
                await batchTranslateViewModel.CancelCommand.ExecuteAsync(null);
                break;
        }
    }

    [RelayCommand]
    private async Task Switch()
    {
        if (CurrentViewModel is not TranslateContentViewModel { IsBusy: true }
            || await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslationModeSwitch"))
        {
            if (CurrentViewModel is TranslateContentViewModel { CurrentTranslateItemModel.IsSaved: false } translateViewModel
                && !string.IsNullOrWhiteSpace(translateViewModel.CurrentTranslateItemModel.TranslatedText)
                && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
            {
                w3Items.First(x => x.Id == translateViewModel.CurrentTranslateItemModel?.Id).Text = translateViewModel.CurrentTranslateItemModel.TranslatedText;
            }
            CurrentViewModel = CurrentViewModel is TranslateContentViewModel
                    ? new BatchTranslateContentViewModel(w3Items, index + 1, appSettings, translator)
                    : new TranslateContentViewModel(w3Items, index, appSettings, translator);
        }
    }

    public TranslateDialogViewModel(IEnumerable<IW3Item> w3Items, int index, IAppSettings appSettings, ITranslator translator)
    {
        this.w3Items = w3Items;
        this.index = index;
        this.appSettings = appSettings;
        this.translator = translator;
        CurrentViewModel = new TranslateContentViewModel(this.w3Items, index, appSettings, translator);
    }
}