using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class SaveResultRecipient : IRecipient<SaveResult>
{
    public void Receive(SaveResult result)
    {
        MessageBox.Show(result.IsSucess ? Strings.SaveSuccess : Strings.SaveFailure, Strings.SaveResult,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}