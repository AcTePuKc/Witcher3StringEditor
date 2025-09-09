using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class AboutDialogViewModel(IReadOnlyDictionary<string, object?> aboutInfo) : ObservableObject, IModalDialogViewModel
{
    public IReadOnlyDictionary<string, object?> AboutInfo => aboutInfo;
    public bool? DialogResult => true;
}