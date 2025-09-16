using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class EditDataDialogViewModel(ITrackableW3StringItem w3StringItem)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public string Title { get; } = string.IsNullOrWhiteSpace(w3StringItem.StrId)
        ? Strings.AddDialogTitle
        : Strings.EditDialogTitle;

    public ITrackableW3StringItem? Item { get; } = w3StringItem.Clone() as ITrackableW3StringItem;

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