using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// BatchTranslateDialog.xaml 的交互逻辑
/// </summary>
public partial class BatchTranslateDialog
{
    private readonly AsyncRequestRecipient<bool> closingRecipient = new();

    public BatchTranslateDialog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(closingRecipient, "BatchTranslateDialogClosing", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatorTranslatingMessage,
                                    Strings.TranslatorTranslatingCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.No);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(closingRecipient);
}