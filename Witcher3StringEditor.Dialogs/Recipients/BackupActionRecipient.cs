using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class BackupActionRecipient : IRecipient<BackupActionMessage>
{
    public void Receive(BackupActionMessage message)
    {
    }
}