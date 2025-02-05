using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.DependencyModel;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class AboutDialogViewModel(Dictionary<string, object> aboutInfo, IEnumerable<RuntimeLibrary>? runtimeLibraries = null) : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public Dictionary<string, object> AboutInfo => aboutInfo;

    public IEnumerable<RuntimeLibrary>? RuntimeLibraries => runtimeLibraries;
}