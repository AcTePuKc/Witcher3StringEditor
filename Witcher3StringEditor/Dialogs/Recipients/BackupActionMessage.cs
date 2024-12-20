using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class BackupActionMessage(BackupActionType backupAction) : AsyncRequestMessage<bool>
{
    public BackupActionType BackupAction { get; } = backupAction;
}