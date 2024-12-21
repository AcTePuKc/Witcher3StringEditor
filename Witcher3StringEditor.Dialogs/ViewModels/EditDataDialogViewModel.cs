using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Dialogs.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class EditDataDialogViewModel(W3Item? w3Item = null)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public string Title { get; } = w3Item == null ? Strings.AddDialogTitle : Strings.EditDialogTitle;

    public W3Item? W3Item { get; } = w3Item != null ? (W3Item)w3Item.Clone() : new W3Item();

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