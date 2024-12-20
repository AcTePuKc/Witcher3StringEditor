using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Core;

public class LogManager
{
    private static readonly Lazy<LogManager> LazyInstance
    = new(static () => new LogManager());

    public static LogManager Instance => LazyInstance.Value;

    public readonly ObservableCollection<LogEvent> LogEvents;

    private LogManager()
    {
        LogEvents = [];
    }

    public void RecordLogEvent(LogEvent logEvent)
    {
        LogEvents.Add(logEvent);
    }
}