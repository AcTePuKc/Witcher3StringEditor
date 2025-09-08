using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class AboutDialogViewModel(IDictionary<string, object?> aboutInfo) : ObservableObject, IModalDialogViewModel
{
    public IDictionary<string, object?> AboutInfo => aboutInfo;
    public bool? DialogResult => true;
}