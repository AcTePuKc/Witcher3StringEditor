using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class LogDialogViewModel : ObservableObject, IModalDialogViewModel
{
    public ObservableCollection<LogEvent> LogEventsogEvents { get; } = LogManager.RetrieveLogEvents();
    public bool? DialogResult => true;
}