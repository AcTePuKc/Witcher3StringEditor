using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class RecentFileOpenedRecipient : IRecipient<RecentFileOpenedMessage>
{
    public void Receive(RecentFileOpenedMessage message)
    {
    }
}