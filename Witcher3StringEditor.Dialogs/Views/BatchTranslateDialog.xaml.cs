using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;
using Windows.Win32;
using Windows.Win32.System.Power;
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
    private readonly AsyncRequestRecipient<bool> requestRecipient = new();
    private readonly NotificationRecipient<bool> notificationRecipient = new();

    public BatchTranslateDialog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "BatchTranslateDialogClosing", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatorTranslatingMessage,
                                    Strings.TranslatorTranslatingCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.No);
        });
        WeakReferenceMessenger.Default.Register<NotificationRecipient<bool>, NotificationMessage<bool>, string>(notificationRecipient, "TranslatorIsBusy", static (r, m) =>
        {
            r.Receive(m);
            if (m.Message)
                PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            else
                PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(requestRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(notificationRecipient);
    }
}