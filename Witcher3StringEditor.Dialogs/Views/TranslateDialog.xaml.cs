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
        var messageHandlers = new[]
        {
            ("TranslatedTextNoSaved", Strings.TranslatedTextNoSavedMessage, Strings.TranslatedTextNoSavedCaption),
            ("TranslationDialogClosing", Strings.TranslatorTranslatingMessage, Strings.TranslatorTranslatingCaption),
            ("TranslationModeSwitch", Strings.TranslationModeSwitchMessage, Strings.TranslationModeSwitchCaption),
            ("TranslationNotEmpty", Strings.TranslationNotEmptyMessage, Strings.TranslationNotEmptyCaption)
        };

        foreach (var (token, message, caption) in messageHandlers)
            WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
                this,
                token,
                (_, m) =>
                {
                    m.Reply(MessageBox.Show(
                        message,
                        caption,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes);
                });
    }

    private void RegisterNotificationMessageHandlers()
    {
        WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
            this,
            "TranslatedTextInvalid",
            (r, m) =>
            {
                _ = MessageBox.Show(Strings.TranslatedTextInvalidMessage,
                    Strings.TranslatedTextInvalidCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });

        WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
            this,
            "TranslateError",
            (r, m) =>
            {
                _ = MessageBox.Show(m.Value,
                    Strings.TranslateErrorCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}