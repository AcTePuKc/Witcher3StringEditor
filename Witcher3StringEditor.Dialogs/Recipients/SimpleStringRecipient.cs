using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class SimpleStringRecipient : IRecipient<SimpleStringMessage>
{
    public void Receive(SimpleStringMessage message)
    {
    }
}