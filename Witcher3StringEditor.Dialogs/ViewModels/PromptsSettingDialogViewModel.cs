using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public class PromptsSettingDialogViewModel(IModelSettings modelSettings) : ObservableObject, IModalDialogViewModel
    {
        public IModelSettings ModelSettings => modelSettings;
        public bool? DialogResult => true;
    }
}
