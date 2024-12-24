using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SaveDialog.xaml 的交互逻辑
/// </summary>
public partial class SaveDialog
{
    private readonly ReturnNothingBooleanRecipient recipient = new();

    public SaveDialog()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ReturnNothingBooleanRecipient, ReturnNothingBooleanMessage>(recipient, (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.Message ? Strings.SaveSuccess : Strings.SaveFailure, Strings.SaveResult, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(recipient);
}