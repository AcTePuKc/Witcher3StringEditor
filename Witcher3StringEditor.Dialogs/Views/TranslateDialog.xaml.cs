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
        var notificationHandlers =
            new (string token, Func<ValueChangedMessage<string>, string> message, string caption)[]
            {
                ("TranslatedTextInvalid", _ => Strings.TranslatedTextInvalidMessage,
                    Strings.TranslatedTextInvalidCaption),
                ("TranslateError", m => m.Value, Strings.TranslateErrorCaption)
            };

        foreach (var (token, message, caption) in notificationHandlers)
            WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
                this,
                token,
                (r, m) =>
                {
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

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    public void Receive(ValueChangedMessage<string> message)
    {
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}