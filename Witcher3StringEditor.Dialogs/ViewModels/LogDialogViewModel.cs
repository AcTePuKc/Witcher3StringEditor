using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class LogDialogViewModel(ObservableCollection<LogEvent> logEvents) : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public ObservableCollection<LogEvent> LogEvents { get; } = logEvents;
}