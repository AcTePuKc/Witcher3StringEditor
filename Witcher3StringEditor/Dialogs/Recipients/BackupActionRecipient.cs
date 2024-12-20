using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Recipients;

public class BackupActionRecipient : IRecipient<BackupActionMessage>
{
    public bool Response { get; private set; }

    public void Receive(BackupActionMessage message)
    {
        Response = MessageBox.Show(message.BackupAction == BackupActionType.restore ?
            Strings.BackupRestoreMessage : Strings.BackupDeleteMessage, Strings.Warning,
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}