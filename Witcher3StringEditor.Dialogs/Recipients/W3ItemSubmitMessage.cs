using CommunityToolkit.Mvvm.Messaging.Messages;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Dialogs.Recipients
{
    public class W3ItemSubmitMessage(IW3Item item) : AsyncRequestMessage<bool>
    {
        public IW3Item Item = item;
    }
}