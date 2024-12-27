using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// TranslateDiaglogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDiaglog
{
    private readonly SimpleStringRecipient recipient = new();

    public TranslateDiaglog()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SimpleStringRecipient, SimpleStringMessage, string>(recipient, "TranslateCharactersNumberExceedLimit", (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(Strings.TranslateCharactersNumberExceedLimit, Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(recipient);
    }
}