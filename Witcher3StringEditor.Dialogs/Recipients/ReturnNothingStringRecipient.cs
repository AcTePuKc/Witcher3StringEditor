using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class ReturnNothingStringRecipient : IRecipient<ReturnNothingStringMessage>
{
    public void Receive(ReturnNothingStringMessage message)
    {
    }
}