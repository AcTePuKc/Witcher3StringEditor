using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class BackupActionMessage(BackupActionType BackupAction) : AsyncRequestMessage<bool>
{
    public BackupActionType BackupAction { get; init; } = BackupAction;
}