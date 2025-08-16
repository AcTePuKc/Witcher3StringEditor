using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class EmbeddedModelSettingsDialogViewModel(IEmbeddedModelSettings modelSettings)
    : ObservableObject, IModalDialogViewModel
{
    public IEmbeddedModelSettings ModelSettings => modelSettings;

    public bool? DialogResult => true;
}