using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class BackupActionRecipient : IRecipient<BackupActionMessage>
{
    public BackupActionType BackupAction { get; set; }

    public void Receive(BackupActionMessage message)
    {
        BackupAction = message.BackupAction;
    }
}