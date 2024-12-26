using CommunityToolkit.Mvvm.Messaging.Messages;
using System.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class WindowClosingMessage(CancelEventArgs e) : AsyncRequestMessage<bool>
{
    public CancelEventArgs Cancel { get; private set; } = e;
}