using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class WindowClosingRecipient : IRecipient<WindowClosingMessage>
{
    public void Receive(WindowClosingMessage message)
    {
    }
}