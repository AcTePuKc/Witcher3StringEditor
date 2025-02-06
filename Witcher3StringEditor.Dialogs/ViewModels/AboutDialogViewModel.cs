using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class AboutDialogViewModel(Dictionary<string, object?> aboutInfo) : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public Dictionary<string, object?> AboutInfo => aboutInfo;
}