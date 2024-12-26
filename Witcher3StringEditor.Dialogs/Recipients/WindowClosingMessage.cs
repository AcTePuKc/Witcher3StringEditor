using CommunityToolkit.Mvvm.Messaging.Messages;
using System.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class WindowClosingMessage(CancelEventArgs e) : AsyncRequestMessage<bool>
{
    public CancelEventArgs Message { get; private set; } = e;
}