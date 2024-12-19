using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Core;

public static class LogManager
{
    private static readonly ObservableCollection<LogEvent> _logEvents = [];

    public static void RecordLogEvent(LogEvent logEvent)
    {
        _logEvents.Add(logEvent);
    }

    public static ObservableCollection<LogEvent> RetrieveLogEvents()
    {
        return _logEvents;
    }
}