using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Witcher3StringEditor.Dialogs.Messaging;

public class FileOpenedMessage(string fileName) : AsyncRequestMessage<bool>
{
    public string FileName { get; } = fileName;
}