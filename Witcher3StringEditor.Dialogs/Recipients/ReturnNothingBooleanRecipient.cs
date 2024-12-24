using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class ReturnNothingBooleanRecipient : IRecipient<ReturnNothingBooleanMessage>
{
    public void Receive(ReturnNothingBooleanMessage message)
    {
    }
}