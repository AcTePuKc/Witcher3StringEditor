using System.Windows;
using Windows.Win32;
using Windows.Win32.System.Power;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Serilog;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     TranslateDialogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDialog
{
    private readonly NotificationRecipient<bool> busyNotificationRecipient = new();
    private readonly NotificationRecipient<string> notificationRecipient = new();
    private readonly AsyncRequestRecipient<bool> requestRecipient = new();

    public TranslateDialog()
    {
        InitializeComponent();
        var notificationHandlers =
            new (string token, Func<NotificationMessage<string>, string> message, string caption)[]
            {
                ("TranslateCharactersNumberExceedLimit", _ => Strings.TranslateCharactersNumberExceedLimitMessage,
                    Strings.TranslateCharactersNumberExceedLimitCaption),
                ("TranslatedTextInvalid", _ => Strings.TranslatedTextInvalidMessage,
                    Strings.TranslatedTextInvalidCaption),
                ("TranslateError", m => m.Message, Strings.TranslateErrorCaption)
            };

        foreach (var (token, message, caption) in notificationHandlers)
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

        var messageHandlers = new[]
        {
            ("TranslatedTextNoSaved", Strings.TranslatedTextNoSavedMessage, Strings.TranslatedTextNoSavedCaption),
            ("TranslationDialogClosing", Strings.TranslatorTranslatingMessage, Strings.TranslatorTranslatingCaption),
            ("TranslationModeSwitch", Strings.TranslationModeSwitchMessage, Strings.TranslationModeSwitchCaption),
            ("TranslationNotEmpty", Strings.TranslationNotEmptyMessage, Strings.TranslationNotEmptyCaption)
        };

        foreach (var (token, message, caption) in messageHandlers)
            WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(
                requestRecipient,
                token,
                (r, m) =>
                {
                    r.Receive(m);
                    m.Reply(MessageBox.Show(
                        message,
                        caption,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes);
                });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(requestRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(notificationRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(busyNotificationRecipient);
    }
}