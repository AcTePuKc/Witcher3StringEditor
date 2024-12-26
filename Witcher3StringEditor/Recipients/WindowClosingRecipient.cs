using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Recipients;

internal class WindowClosingRecipient : IRecipient<WindowClosingMessage>
{
    public void Receive(WindowClosingMessage message)
    {
    }
}