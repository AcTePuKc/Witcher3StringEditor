using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class DeleteDataDialogViewModel(ILogger<DeleteDataDialogViewModel> logger, IEnumerable<IW3Item> w3Items)
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    public IEnumerable<IW3Item> W3Items { get; } = w3Items;

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private void Delete()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
        logger.LogInformation("The selected W3Items have been deleted.");
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
        logger.LogInformation("The selected W3Items have not been deleted.");
    }
}