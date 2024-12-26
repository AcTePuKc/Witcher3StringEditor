using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public partial class BatchTranslateDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;
    }
}