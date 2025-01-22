using CommunityToolkit.Mvvm.Messaging;
using Serilog.Events;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class LogEventRecipient : IRecipient<LogEvent>
{
    public void Receive(LogEvent message)
    {
    }
}