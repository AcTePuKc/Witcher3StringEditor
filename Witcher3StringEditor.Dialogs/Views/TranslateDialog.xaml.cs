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
/// TranslateDiaglogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDialog
{
    private readonly AsyncRequestRecipient<bool> requestRecipient = new();
    private readonly NotificationRecipient<string> notificationRecipient = new();
    private readonly NotificationRecipient<bool> translatorBusyNotificationRecipient = new();

    public TranslateDialog()
    {
        InitializeComponent();
        var notificationHandlers = new (string token, Func<NotificationMessage<string>, string> message, string caption)[]
        {
            ("TranslateCharactersNumberExceedLimit", m => Strings.TranslateCharactersNumberExceedLimitMessage, Strings.TranslateCharactersNumberExceedLimitCaption),
            ("TranslatedTextInvalid", m => Strings.TranslatedTextInvalidMessage, Strings.TranslatedTextInvalidCaption),
            ("TranslateError", m => m.Message, Strings.TranslateErrorCaption)
        };

        foreach ((string token, Func<NotificationMessage<string>, string> message, string caption) in notificationHandlers)
        {
            WeakReferenceMessenger.Default.Register<NotificationRecipient<string>, NotificationMessage<string>, string>(
                notificationRecipient,
                token,
                (r, m) =>
                {
                    r.Receive(m);
                    _ = MessageBox.Show(message.Invoke(m),
                                      caption,
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                });
        }

        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "TranslatedTextNoSaved", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatedTextNoSavedMessage,
                                    Strings.TranslatedTextNoSavedCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "TranslationDialogClosing", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatorTranslatingMessage,
                                    Strings.TranslatorTranslatingCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<NotificationRecipient<bool>, NotificationMessage<bool>, string>(translatorBusyNotificationRecipient, "TranslatorIsBatchTranslating", (r, m) =>
        {
            r.Receive(m);
            if (m.Message)
                PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            else
                PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            SwitchBtn.IsEnabled = !m.Message;
        });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "TranslationModeSwitch", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslationModeSwitchMessage,
                        Strings.TranslationModeSwitchCaption,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(requestRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(notificationRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(translatorBusyNotificationRecipient);
    }
}