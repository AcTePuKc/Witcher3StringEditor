using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Core
{
    public static class LogManger
    {
        private static ObservableCollection<LogEvent> LogEvents { get; } = [];

        public static void Log(LogEvent logEvent) => LogEvents.Add(logEvent);

        public static ObservableCollection<LogEvent> GetLogs() => LogEvents;
    }
}