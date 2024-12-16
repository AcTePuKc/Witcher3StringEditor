using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class EditDataDialogViewModel(W3ItemModel? w3Item = null)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public string Title { get; } = w3Item == null ? Strings.AddDialogTitle : Strings.EditDialogTitle;

    public W3ItemModel? W3Item { get; } = w3Item != null ? (W3ItemModel)w3Item.Clone() : new W3ItemModel();

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private void Submit()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}