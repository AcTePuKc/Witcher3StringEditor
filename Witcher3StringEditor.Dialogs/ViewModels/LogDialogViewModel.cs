using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels;

/// <summary>
///     ViewModel for the log dialog window
///     Manages the display of log events in the UI and synchronizes with the source log collection
///     Implements IModalDialogViewModel to support dialog result handling
/// </summary>
public class LogDialogViewModel
    : ObservableObject, IModalDialogViewModel
{
    /// <summary>
    ///     The source collection of log events to display
    /// </summary>
    private readonly ObservableCollection<LogEvent> sourceLogEvents;

    /// <summary>
    ///     Initializes a new instance of the LogDialogViewModel class
    /// </summary>
    /// <param name="logEvents">The source collection of log events to display</param>
    public LogDialogViewModel(ObservableCollection<LogEvent> logEvents)
    {
        sourceLogEvents = logEvents;
        sourceLogEvents.ForEach(x => LogEvents.Add(new LogEventItemModel(x)));
        WeakEventManager<ObservableCollection<LogEvent>, NotifyCollectionChangedEventArgs>
            .AddHandler(sourceLogEvents, nameof(ObservableCollection<LogEvent>.CollectionChanged),
                OnSourceLogsCollectionChanged);
        WeakEventManager<ObservableCollection<LogEventItemModel>, NotifyCollectionChangedEventArgs>
            .AddHandler(LogEvents, nameof(ObservableCollection<LogEventItemModel>.CollectionChanged),
                OnLogEventsCollectionChanged);
    }

    /// <summary>
    ///     Gets the collection of log events for display in the UI
    /// </summary>
    public ObservableCollection<LogEventItemModel> LogEvents { get; } = [];

    /// <summary>
    ///     Gets the dialog result value
    ///     Returns true to indicate that the dialog was closed successfully
    /// </summary>
    public bool? DialogResult => true;

    /// <summary>
    ///     Handles changes to the source log events collection
    ///     Adds new log events to the UI collection when items are added to the source collection
    /// </summary>
    /// <param name="sender">The source collection</param>
    /// <param name="e">The collection change event arguments</param>
    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnSourceLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is not { Action: NotifyCollectionChangedAction.Add, NewItems: not null }) return;
        foreach (LogEvent item in e.NewItems)
            await Application.Current.Dispatcher.InvokeAsync(() => LogEvents.Add(new LogEventItemModel(item)));
    }

    /// <summary>
    ///     Handles changes to the UI log events collection
    ///     Removes log events from the source collection when items are removed from the UI collection
    /// </summary>
    /// <param name="sender">The UI collection</param>
    /// <param name="e">The collection change event arguments</param>
    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnLogEventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is not { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }) return;
        foreach (LogEventItemModel item in e.OldItems)
            await Application.Current.Dispatcher.InvokeAsync(() => sourceLogEvents.Remove(item.EventEntry));
    }
}