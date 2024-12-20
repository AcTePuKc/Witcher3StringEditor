using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class SaveResultRecipient : IRecipient<SaveResultMessage>
{
    public void Receive(SaveResultMessage result)
    {
        MessageBox.Show(result.IsSucess ? Strings.SaveSuccess : Strings.SaveFailure, Strings.SaveResult,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}