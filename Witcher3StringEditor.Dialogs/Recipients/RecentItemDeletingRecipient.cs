using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class RecentItemDeletingRecipient : IRecipient<RecentItemDeletingMessage>
{
    public void Receive(RecentItemDeletingMessage message)
    {
    }
}