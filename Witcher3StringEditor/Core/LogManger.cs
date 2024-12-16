using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Core
{
    public static class LogManger
    {
        public static ObservableCollection<LogEvent> Logs { get; } = [];

        public static void Log(LogEvent logEvent) => Logs.Add(logEvent);
    }
}