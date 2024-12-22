using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs;
using Witcher3StringEditor.Dialogs.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class SaveResultRecipient : IRecipient<SaveResultMessage>
{
    public bool IsSucess {  get; set; }

    public void Receive(SaveResultMessage result)
    {
;
    }
}