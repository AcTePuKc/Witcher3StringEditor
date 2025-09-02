using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class EditDataDialogViewModel(IEditW3Item w3Item)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public string Title { get; } = string.IsNullOrWhiteSpace(w3Item.StrId)
        ? LangKeys.AddDialogTitle
        : LangKeys.EditDialogTitle;

    public IEditW3Item? W3Item { get; } = w3Item.Clone() as IEditW3Item;

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