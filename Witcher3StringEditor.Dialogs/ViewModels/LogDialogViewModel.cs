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
        logEvents.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (LogEvent item in e.NewItems)
                {
                    LogEvents.Add(new LogEventItem(item));
                }
            }
        };
        LogEvents.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (LogEventItem item in e.OldItems)
                {
                    logEvents.Remove(item.EventEntry);
                }
            }
        };
    }
}