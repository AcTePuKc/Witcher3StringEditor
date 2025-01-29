using Serilog.Events;

namespace Witcher3StringEditor.Dialogs.Models;

public class LogEventItem(LogEvent logEvent)
{
    public LogEvent EventEntry => logEvent;

    public DateTimeOffset Timestamp => logEvent.Timestamp;

    public LogEventLevel Level => logEvent.Level;

    public string Message => logEvent.RenderMessage();
}