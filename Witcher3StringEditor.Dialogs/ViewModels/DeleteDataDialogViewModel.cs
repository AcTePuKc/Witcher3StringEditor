using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class DeleteDataDialogViewModel(IEnumerable<IW3StringItem> w3StringItems)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public IEnumerable<IW3StringItem> W3StringItems { get; } = w3StringItems;

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private void Delete()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
        Log.Information("The selected W3Items have been deleted.");
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
        Log.Information("The selected W3Items have not been deleted.");
    }
}