using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class BackupRecipient : IRecipient<BackupMessage>
{
    public void Receive(BackupMessage message)
    {
    }
}