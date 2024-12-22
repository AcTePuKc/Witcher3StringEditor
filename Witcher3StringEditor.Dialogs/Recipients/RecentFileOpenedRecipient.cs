using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients
{
    public class RecentFileOpenedRecipient : IRecipient<RecentFileOpenedMessage>
    {
        public string? FileName { get; private set; }

        public void Receive(RecentFileOpenedMessage message)
        {
            FileName = message.FileName;
        }
    }
}