using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    internal partial class LogDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;
    }
}