using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class NotificationRecipient<T> : IRecipient<NotificationMessage<T>>
{
    public void Receive(NotificationMessage<T> message)
    {
    }
}