using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class FileOpenedMessage(string FileName) : AsyncRequestMessage<bool>
{
    public string FileName { get; } = FileName;
}