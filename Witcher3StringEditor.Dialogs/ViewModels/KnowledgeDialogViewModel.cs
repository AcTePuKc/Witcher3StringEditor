using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public class KnowledgeDialogViewModel(IKnowledgeService knowledgeService) : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;
    }
}
