using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class ReloadW3ItemsRecipient : IRecipient<ReloadW3ItemsMessage>
{
    public bool Response { get; private set; }

    public void Receive(ReloadW3ItemsMessage message)
    {
        Response = MessageBox.Show("GGG", "Q", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}