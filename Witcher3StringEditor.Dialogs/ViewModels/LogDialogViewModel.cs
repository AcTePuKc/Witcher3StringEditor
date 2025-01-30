using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class LogDialogViewModel
    : ObservableObject, IModalDialogViewModel
{
    public bool? DialogResult => true;

    public ObservableCollection<LogEventItem> LogEvents { get; } = [];

    public LogDialogViewModel(ObservableCollection<LogEvent> logEvents)
    {
        foreach (var logEvent in logEvents)
            LogEvents.Add(new LogEventItem(logEvent));
        logEvents.CollectionChanged += (_, e) =>
        {
            if (e is not { Action: NotifyCollectionChangedAction.Add, NewItems: not null }) return;
            foreach (LogEvent item in e.NewItems)
            {
                LogEvents.Add(new LogEventItem(item));
            }
        };
        LogEvents.CollectionChanged += (_, e) =>
        {
            if (e is not { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }) return;
            foreach (LogEventItem item in e.OldItems)
            {
                logEvents.Remove(item.EventEntry);
            }
        };
    }
}