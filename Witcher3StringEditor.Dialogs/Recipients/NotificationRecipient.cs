using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Dialogs.Recipients
{
    internal class NotificationRecipient<T> : IRecipient<NotificationMessage<T>>
    {
        public void Receive(NotificationMessage<T> message)
        {
        }
    }
}