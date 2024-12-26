using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients
{
    public class FileOpenedRecipient : IRecipient<FileOpenedMessage>
    {
        public void Receive(FileOpenedMessage message)
        {
        }
    }
}