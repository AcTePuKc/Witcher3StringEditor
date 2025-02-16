using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class NotificationRecipient<T> : IRecipient<NotificationMessage<T>>
{
    public void Receive(NotificationMessage<T> message)
    {
    }
}