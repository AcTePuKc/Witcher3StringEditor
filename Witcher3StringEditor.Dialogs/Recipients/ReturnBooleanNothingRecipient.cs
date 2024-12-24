using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class ReturnBooleanNothingRecipient : IRecipient<ReturnBooleanNothingMessage>
{
    public void Receive(ReturnBooleanNothingMessage message)
    {
    }
}