using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
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
public sealed class LogDialogViewModel
    : ObservableObject, IModalDialogViewModel, IDisposable
{
    /// <summary>
    ///     The source collection of log events to display
    /// </summary>
    private readonly ObservableCollection<LogEvent> sourceLogEvents;

    private bool disposedValue;

    /// <summary>
    ///     Initializes a new instance of the LogDialogViewModel class
    /// </summary>
    /// <param name="logEvents">The source collection of log events to display</param>
    public LogDialogViewModel(ObservableCollection<LogEvent> logEvents)
    {
        sourceLogEvents = logEvents;
        LogEvents.CollectionChanged += OnLogEventsCollectionChanged;
        sourceLogEvents.CollectionChanged += OnSourceLogsCollectionChanged;
        sourceLogEvents.ForEach(x => LogEvents.Add(new LogEventItemModel(x)));
    }

    /// <summary>
    ///     Gets the collection of log events for display in the UI
    /// </summary>
    public ObservableCollection<LogEventItemModel> LogEvents { get; } = [];

    public void Dispose()
    {
        Dispose(true);
    }

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
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => LogEvents.Add(new LogEventItemModel(item)));
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
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => sourceLogEvents.Remove(item.EventEntry));
    }

    
    private void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
        {
            LogEvents.CollectionChanged -= OnLogEventsCollectionChanged;
            sourceLogEvents.CollectionChanged -= OnSourceLogsCollectionChanged;
        }

        disposedValue = true;
    }
}