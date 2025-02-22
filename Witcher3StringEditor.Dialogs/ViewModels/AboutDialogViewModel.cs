using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class AboutDialogViewModel(Dictionary<string, object?> aboutInfo) : ObservableObject, IModalDialogViewModel
{
    public Dictionary<string, object?> AboutInfo => aboutInfo;
    public bool? DialogResult => true;
}