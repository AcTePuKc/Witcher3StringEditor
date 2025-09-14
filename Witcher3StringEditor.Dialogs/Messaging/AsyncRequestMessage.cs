using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Messaging;

public class AsyncRequestMessage<TRequest, TResponse>(TRequest request) : AsyncRequestMessage<TResponse>
{
    public TRequest Request { get; } = request;
}