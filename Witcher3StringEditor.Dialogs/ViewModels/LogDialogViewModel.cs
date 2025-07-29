using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class LogDialogViewModel
    : ObservableObject, IModalDialogViewModel
{
    public LogDialogViewModel(ObservableCollection<LogEvent> logEvents)
    {
        foreach (var logEvent in logEvents)
            LogEvents.Add(new LogEventItem(logEvent));
        logEvents.CollectionChanged += async (_, e) =>
        {
            if (e is not { Action: NotifyCollectionChangedAction.Add, NewItems: not null }) return;
            foreach (LogEvent item in e.NewItems)
                await Application.Current.Dispatcher.BeginInvoke(() => LogEvents.Add(new LogEventItem(item)));
        };
        LogEvents.CollectionChanged += async (_, e) =>
        {
            if (e is not { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }) return;
            foreach (LogEventItem item in e.OldItems)
                await Application.Current.Dispatcher.BeginInvoke(() => logEvents.Remove(item.EventEntry));
        };
    }

    public ObservableCollection<LogEventItem> LogEvents { get; } = [];
    public bool? DialogResult => true;
}