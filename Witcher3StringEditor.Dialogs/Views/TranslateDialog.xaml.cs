using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     TranslateDialogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDialog : IRecipient<ValueChangedMessage<string>>, IRecipient<AsyncRequestMessage<bool>>
{
    public TranslateDialog()
    {
        InitializeComponent();
        RegisterMessageHandlers();
    }

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    public void Receive(ValueChangedMessage<string> message)
    {
    }

    private void RegisterMessageHandlers()
    {
        RegisterNotificationMessageHandlers();
        RegisterAsyncRequestMessageHandlers();
    }

    private void RegisterAsyncRequestMessageHandlers()
    {
        var messageHandlers = CreateAsyncRequestHandlers();
        foreach (var (token, message, caption) in messageHandlers)
            RegisterAsyncRequestHandler(token, message, caption);
    }

    private void RegisterAsyncRequestHandler(string token, Func<string> message, Func<string> caption)
    {
        WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
            this,
            token,
            (_, m) =>
            {
                m.Reply(MessageBox.Show(
                    message(),
                    caption(),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes);
            });
    }

    private static (string, Func<string>, Func<string>)[] CreateAsyncRequestHandlers()
    {
        return
        [
            ("TranslatedTextNoSaved", () => Strings.TranslatedTextNoSavedMessage,
                () => Strings.TranslatedTextNoSavedCaption),
            ("TranslationDialogClosing", () => Strings.TranslatorTranslatingMessage,
                () => Strings.TranslatorTranslatingCaption),
            ("TranslationModeSwitch", () => Strings.TranslationModeSwitchMessage,
                () => Strings.TranslationModeSwitchCaption),
            ("TranslationNotEmpty", () => Strings.TranslationNotEmptyMessage, () => Strings.TranslationNotEmptyCaption)
        ];
    }

    private void RegisterNotificationMessageHandlers()
    {
        var notificationHandlers = CreateNotificationHandlers();
        foreach (var (token, message, caption) in notificationHandlers)
            RegisterNotificationHandler(token, message, caption);
    }

    private void RegisterNotificationHandler(string token, Func<ValueChangedMessage<string>, string> message,
        Func<string> caption)
    {
        WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
            this,
            token,
            (r, m) =>
            {
                _ = MessageBox.Show(message.Invoke(m),
                    caption(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
    }

    private static (string, Func<ValueChangedMessage<string>, string>, Func<string>)[] CreateNotificationHandlers()
    {
        return
        [
            ("TranslatedTextInvalid", _ => Strings.TranslatedTextInvalidMessage,
                () => Strings.TranslatedTextInvalidCaption),
            ("TranslateError", m => m.Value, () => Strings.TranslateErrorCaption)
        ];
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}