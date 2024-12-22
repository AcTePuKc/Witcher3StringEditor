using CommunityToolkit.Mvvm.Messaging;
using Serilog.Events;

namespace Witcher3StringEditor.Recipients;

internal class LogEventRecipient : IRecipient<LogEvent>
{
    public void Receive(LogEvent message)
    {

    }
}