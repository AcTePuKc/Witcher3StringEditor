using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class AsyncRequestRecipient<T> : IRecipient<AsyncRequestMessage<T>>
{
    public void Receive(AsyncRequestMessage<T> message)
    {
    }
}