using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SettingsDialog.xaml 的交互逻辑
/// </summary>
public partial class SettingsDialog
{
    private readonly AsyncRequestRecipient<bool> closingRecipient = new();

    public SettingsDialog()
    {
        InitializeComponent();

        var messageHandlers = new[]
        {
            ("InitializationIncomplete", Strings.InitializationIncompleteMessage,
                Strings.InitializationIncompleteCaption)
        };

        foreach (var (token, message, caption) in messageHandlers)
            WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(
                closingRecipient,
                token,
                (r, m) =>
                {
                    r.Receive(m);
                    m.Reply(MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes);
                });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(closingRecipient);
    }
}