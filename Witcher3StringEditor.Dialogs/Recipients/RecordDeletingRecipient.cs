using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class RecordDeletingRecipient : IRecipient<RecordDeletingMessage>
{
    public void Receive(RecordDeletingMessage message)
    {
    }
}