using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Recipients;

internal class OpenFileRecipient : IRecipient<OpenFileMessage>
{
    public void Receive(OpenFileMessage message)
    {
    }
}