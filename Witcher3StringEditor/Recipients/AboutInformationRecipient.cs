using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Recipients;

internal class AboutInformationRecipient : IRecipient<AboutInformationMessage>
{
    public void Receive(AboutInformationMessage aboutInformation) 
        => MessageBox.Show(aboutInformation.Message, Strings.About, MessageBoxButton.OK, MessageBoxImage.Information);
}