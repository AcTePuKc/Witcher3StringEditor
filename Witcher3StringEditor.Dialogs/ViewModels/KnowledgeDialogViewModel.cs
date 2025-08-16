using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public class KnowledgeDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;
    }
}
