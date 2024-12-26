using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class EditDataDialogViewModel(IW3Item w3Item)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public string Title { get; } = w3Item.StrId == string.Empty ? Strings.AddDialogTitle : Strings.EditDialogTitle;

    public IW3Item? W3Item { get; } = w3Item?.Clone() as IW3Item;

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