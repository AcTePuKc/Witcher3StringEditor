namespace Witcher3StringEditor.Dialogs.Recipients;

public class NotificationMessage<T>(T message)
{
    public T Message { get; } = message;
}