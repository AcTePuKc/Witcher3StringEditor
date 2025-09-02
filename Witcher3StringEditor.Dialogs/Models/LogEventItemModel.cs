using JetBrains.Annotations;
using Serilog.Events;

namespace Witcher3StringEditor.Dialogs.Models;

public class LogEventItemModel(LogEvent logEvent)
{
    public LogEvent EventEntry => logEvent;

    [UsedImplicitly] public DateTimeOffset Timestamp => logEvent.Timestamp;

    [UsedImplicitly] public LogEventLevel Level => logEvent.Level;

    [UsedImplicitly] public string Message => logEvent.RenderMessage();
}