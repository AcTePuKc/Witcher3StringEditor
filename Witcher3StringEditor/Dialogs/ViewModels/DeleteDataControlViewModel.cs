using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    internal partial class DeleteDataDialogViewModel(IEnumerable<W3ItemModel> w3Items) : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public event EventHandler? RequestClose;

        public bool? DialogResult { get; private set; }

        public IEnumerable<W3ItemModel> W3Items { get; } = w3Items;

        [RelayCommand]
        private void Delete()
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
}