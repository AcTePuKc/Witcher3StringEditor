using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class SaveResultRecipient : IRecipient<SaveResultMessage>
{
    public void Receive(SaveResultMessage message)
    {
    }
}