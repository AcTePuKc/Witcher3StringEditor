namespace Witcher3StringEditor.Dialogs.Recipients;

internal class NotificationMessage<T>(T message)
{
    public T Message { get; } = message;
}