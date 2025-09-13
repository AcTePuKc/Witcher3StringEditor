using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public class LogDialogViewModel
    : ObservableObject, IModalDialogViewModel
{
    private readonly ObservableCollection<LogEvent> _sourceLogs;

    public LogDialogViewModel(ObservableCollection<LogEvent> logEvents)
    {
        _sourceLogs = logEvents;
        _sourceLogs.ForEach(x => LogEvents.Add(new LogEventItemModel(x)));
        WeakEventManager<ObservableCollection<LogEvent>, NotifyCollectionChangedEventArgs>
            .AddHandler(_sourceLogs, nameof(ObservableCollection<LogEvent>.CollectionChanged), OnSourceLogsCollectionChanged);
        WeakEventManager<ObservableCollection<LogEventItemModel>, NotifyCollectionChangedEventArgs>
            .AddHandler(LogEvents, nameof(ObservableCollection<LogEventItemModel>.CollectionChanged), OnLogEventsCollectionChanged);
    }

    public ObservableCollection<LogEventItemModel> LogEvents { get; } = [];

    public bool? DialogResult => true;

    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnSourceLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is not { Action: NotifyCollectionChangedAction.Add, NewItems: not null }) return;
        foreach (LogEvent item in e.NewItems)
            await Application.Current.Dispatcher.InvokeAsync(() => LogEvents.Add(new LogEventItemModel(item)));
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnLogEventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is not { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }) return;
        foreach (LogEventItemModel item in e.OldItems)
            await Application.Current.Dispatcher.InvokeAsync(() => _sourceLogs.Remove(item.EventEntry));
    }
}