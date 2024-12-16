using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using Witcher3StringEditor.Core;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class LogDialogViewModel : ObservableObject, IModalDialogViewModel
{
    public ObservableCollection<LogEvent> LogEventsogEvents { get; } = LogManger.GetLogs();
    public bool? DialogResult => true;
}