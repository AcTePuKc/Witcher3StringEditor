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
    private object current;

    private readonly int index;

    private readonly IEnumerable<IW3Item> w3Items;

    private readonly IAppSettings appSettings;

    private readonly ITranslator translator;

    [RelayCommand]
    private async Task Closing(CancelEventArgs e)
    {
        if (Current is TranslateViewModel translateViewModel)
        {
            if (translateViewModel.IsBusy && !await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatorIsBusy"))
            {
                e.Cancel = true;
            }
            else
            {
                if (translateViewModel.CurrentTranslateItemModel is { IsSaved: false }
                    && !string.IsNullOrWhiteSpace(translateViewModel.CurrentTranslateItemModel.TranslatedText)
                    && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "TranslatedTextNoSaved"))
                    w3Items.First(x => x.Id == translateViewModel.CurrentTranslateItemModel.Id).Text = translateViewModel.CurrentTranslateItemModel.TranslatedText;
            }
        }
        else if (Current is BatchTranslateViewModel batchTranslateViewModel && batchTranslateViewModel.IsBusy)
        {
            if (await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "BatchTranslateDialogClosing"))
            {
                e.Cancel = true;
            }
            else
            {
                await batchTranslateViewModel.CancelCommand.ExecuteAsync(null);
            }
        }
    }

    [RelayCommand]
    private void Switch()
    {
        Current = Current is TranslateViewModel
            ? new BatchTranslateViewModel(w3Items, index, appSettings, translator)
            : new TranslateViewModel(w3Items, index, appSettings, translator);
    }

    public TranslateDialogViewModel(IEnumerable<IW3Item> w3Items, int index, IAppSettings appSettings, ITranslator translator)
    {
        this.w3Items = w3Items;
        this.index = index;
        this.appSettings = appSettings;
        this.translator = translator;
        Current = new TranslateViewModel(w3Items, index, appSettings, translator);
    }
}