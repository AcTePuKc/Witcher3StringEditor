using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients
{
    public class RecentFileIsPinRecipient : IRecipient<RecentFileOpenedMessage>
    {
        public void Receive(RecentFileOpenedMessage message)
        {
        }
    }
}