using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class LogDialogViewModel(IEnumerable<LogEvent> logEvents) 
    : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public IEnumerable<LogEvent> LogEvents { get; } = logEvents;
}