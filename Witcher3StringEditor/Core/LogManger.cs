using Serilog.Events;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Core;

public class LogManager
{
    private bool isProcessing;

    private readonly Queue<LogEvent> logQueue = new();

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
        lock (logQueue)
        {
            logQueue.Enqueue(logEvent);
            if (isProcessing) return;
            isProcessing = true;
            ProcessLogQueue();
        }
    }

    private void ProcessLogQueue()
    {
        while (true)
        {
            LogEvent logEvent;
            lock (logQueue)
            {
                if (logQueue.Count == 0)
                {
                    isProcessing = false;
                    return;
                }

                logEvent = logQueue.Dequeue();
            }

            LogEvents.Add(logEvent);
        }
    }
}